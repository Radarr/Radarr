using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMoveMovieFiles
    {
        MovieFile MoveMovieFile(MovieFile movieFile, Movie movie);
        MovieFile MoveMovieFile(MovieFile movieFile, LocalMovie localMovie);
        MovieFile CopyMovieFile(MovieFile movieFile, LocalMovie localMovie);
    }

    public class MovieFileMovingService : IMoveMovieFiles
    {
        private readonly IUpdateMovieFileService _updateMovieFileService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IImportScript _scriptImportDecider;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MovieFileMovingService(IUpdateMovieFileService updateMovieFileService,
                                IBuildFileNames buildFileNames,
                                IDiskTransferService diskTransferService,
                                IDiskProvider diskProvider,
                                IMediaFileAttributeService mediaFileAttributeService,
                                IImportScript scriptImportDecider,
                                IRootFolderService rootFolderService,
                                IEventAggregator eventAggregator,
                                IConfigService configService,
                                Logger logger)
        {
            _updateMovieFileService = updateMovieFileService;
            _buildFileNames = buildFileNames;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _mediaFileAttributeService = mediaFileAttributeService;
            _scriptImportDecider = scriptImportDecider;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public MovieFile MoveMovieFile(MovieFile movieFile, Movie movie)
        {
            var newFileName = _buildFileNames.BuildFileName(movie, movieFile);
            var filePath = _buildFileNames.BuildFilePath(movie, newFileName, Path.GetExtension(movieFile.RelativePath));

            EnsureMovieFolder(movieFile, movie, filePath);

            _logger.Debug("Renaming movie file: {0} to {1}", movieFile, filePath);

            return TransferFile(movieFile, movie, filePath, TransferMode.Move);
        }

        public MovieFile MoveMovieFile(MovieFile movieFile, LocalMovie localMovie)
        {
            var newFileName = _buildFileNames.BuildFileName(localMovie.Movie, movieFile, null, localMovie.CustomFormats);
            var filePath = _buildFileNames.BuildFilePath(localMovie.Movie, newFileName, Path.GetExtension(localMovie.Path));

            EnsureMovieFolder(movieFile, localMovie, filePath);

            _logger.Debug("Moving movie file: {0} to {1}", movieFile.Path, filePath);

            return TransferFile(movieFile, localMovie.Movie, filePath, TransferMode.Move, localMovie);
        }

        public MovieFile CopyMovieFile(MovieFile movieFile, LocalMovie localMovie)
        {
            var newFileName = _buildFileNames.BuildFileName(localMovie.Movie, movieFile, null, localMovie.CustomFormats);
            var filePath = _buildFileNames.BuildFilePath(localMovie.Movie, newFileName, Path.GetExtension(localMovie.Path));

            EnsureMovieFolder(movieFile, localMovie, filePath);

            if (_configService.CopyUsingHardlinks)
            {
                _logger.Debug("Attempting to hardlink movie file: {0} to {1}", movieFile.Path, filePath);
                return TransferFile(movieFile, localMovie.Movie, filePath, TransferMode.HardLinkOrCopy, localMovie);
            }

            _logger.Debug("Copying movie file: {0} to {1}", movieFile.Path, filePath);
            return TransferFile(movieFile, localMovie.Movie, filePath, TransferMode.Copy, localMovie);
        }

        private MovieFile TransferFile(MovieFile movieFile, Movie movie, string destinationFilePath, TransferMode mode, LocalMovie localMovie = null)
        {
            Ensure.That(movieFile, () => movieFile).IsNotNull();
            Ensure.That(movie, () => movie).IsNotNull();
            Ensure.That(destinationFilePath, () => destinationFilePath).IsValidPath(PathValidationType.CurrentOs);

            var movieFilePath = movieFile.Path ?? Path.Combine(movie.Path, movieFile.RelativePath);

            if (!_diskProvider.FileExists(movieFilePath))
            {
                throw new FileNotFoundException("Movie file path does not exist", movieFilePath);
            }

            if (movieFilePath == destinationFilePath)
            {
                throw new SameFilenameException("File not moved, source and destination are the same", movieFilePath);
            }

            movieFile.RelativePath = movie.Path.GetRelativePath(destinationFilePath);

            if (localMovie is not null)
            {
                localMovie.FileNameBeforeRename = movieFile.RelativePath;
            }

            if (localMovie is not null && _scriptImportDecider.TryImport(movieFilePath, destinationFilePath, localMovie, movieFile, mode) is var scriptImportDecision && scriptImportDecision != ScriptImportDecision.DeferMove)
            {
                if (scriptImportDecision == ScriptImportDecision.RenameRequested)
                {
                    try
                    {
                        MoveMovieFile(movieFile, movie);
                    }
                    catch (SameFilenameException)
                    {
                        _logger.Debug("No rename was required. File already exists at destination.");
                    }
                }
            }
            else
            {
                _diskTransferService.TransferFile(movieFilePath, destinationFilePath, mode);
            }

            _updateMovieFileService.ChangeFileDateForFile(movieFile, movie);

            try
            {
                _mediaFileAttributeService.SetFolderLastWriteTime(movie.Path, movieFile.DateAdded);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to set last write time");
            }

            _mediaFileAttributeService.SetFilePermissions(destinationFilePath);

            return movieFile;
        }

        private void EnsureMovieFolder(MovieFile movieFile, LocalMovie localMovie, string filePath)
        {
            EnsureMovieFolder(movieFile, localMovie.Movie, filePath);
        }

        private void EnsureMovieFolder(MovieFile movieFile, Movie movie, string filePath)
        {
            var movieFileFolder = Path.GetDirectoryName(filePath);
            var movieFolder = movie.Path;
            var rootFolder = _rootFolderService.GetBestRootFolderPath(movieFolder);

            if (rootFolder.IsNullOrWhiteSpace())
            {
                throw new RootFolderNotFoundException($"Root folder was not found, '{movieFolder}' is not a subdirectory of a defined root folder.");
            }

            if (!_diskProvider.FolderExists(rootFolder))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolder}' was not found.");
            }

            var changed = false;
            var newEvent = new MovieFolderCreatedEvent(movie, movieFile);

            if (!_diskProvider.FolderExists(movieFolder))
            {
                CreateFolder(movieFolder);
                newEvent.MovieFolder = movieFolder;
                changed = true;
            }

            if (movieFolder != movieFileFolder && !_diskProvider.FolderExists(movieFileFolder))
            {
                CreateFolder(movieFileFolder);
                newEvent.MovieFileFolder = movieFileFolder;
                changed = true;
            }

            if (changed)
            {
                _eventAggregator.PublishEvent(newEvent);
            }
        }

        private void CreateFolder(string directoryName)
        {
            Ensure.That(directoryName, () => directoryName).IsNotNullOrWhiteSpace();

            var parentFolder = new OsPath(directoryName).Directory.FullPath;
            if (!_diskProvider.FolderExists(parentFolder))
            {
                CreateFolder(parentFolder);
            }

            try
            {
                _diskProvider.CreateFolder(directoryName);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to create directory: {0}", directoryName);
            }

            _mediaFileAttributeService.SetFolderPermissions(directoryName);
        }
    }
}
