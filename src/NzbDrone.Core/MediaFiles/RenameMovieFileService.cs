using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRenameMovieFileService
    {
        List<RenameMovieFilePreview> GetRenamePreviews(int movieId);
        void RenameMoviePath(Movie movie, bool shouldRenameFiles);
    }

    public class RenameMovieFileService : IRenameMovieFileService,
                                          IExecute<RenameFilesCommand>,
                                          IExecute<RenameMovieCommand>,
                    IExecute<RenameMovieFolderCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveMovieFiles _movieFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly Logger _logger;

        public RenameMovieFileService(IMovieService movieService,
                                      IMediaFileService mediaFileService,
                                      IMoveMovieFiles movieFileMover,
                                      IEventAggregator eventAggregator,
                                      IBuildFileNames filenameBuilder,
                                      IConfigService configService,
                                      IRecycleBinProvider recycleBinProvider,
                                      IDiskProvider diskProvider,
                                      Logger logger)
        {
            _movieService = movieService;
            _mediaFileService = mediaFileService;
            _movieFileMover = movieFileMover;
            _eventAggregator = eventAggregator;
            _filenameBuilder = filenameBuilder;
            _configService = configService;
            _recycleBinProvider = recycleBinProvider;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameMovieFilePreview> GetRenamePreviews(int movieId)
        {
            var movie = _movieService.GetMovie(movieId);
            var file = _mediaFileService.GetFilesByMovie(movieId);

            return GetPreviews(movie, file).OrderByDescending(m => m.MovieId).ToList(); //TODO: Would really like to not have these be lists
        }

        private IEnumerable<RenameMovieFilePreview> GetPreviews(Movie movie, List<MovieFile> files)
        {
            foreach (var file in files)
            {
                var movieFilePath = Path.Combine(movie.Path, file.RelativePath);

                var newName = _filenameBuilder.BuildFileName(movie, file);
                var newPath = _filenameBuilder.BuildFilePath(movie, newName, Path.GetExtension(movieFilePath));

                if (!movieFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameMovieFilePreview
                    {
                        MovieId = movie.Id,
                        MovieFileId = file.Id,
                        ExistingPath = movieFilePath,

                        //NewPath = movie.Path.GetRelativePath(newPath)
                        NewPath = newPath
                    };
                }
            }
        }

        private void RenameFiles(List<MovieFile> movieFiles, Movie movie, string oldMoviePath = null)
        {
            var renamed = new List<MovieFile>();

            if (oldMoviePath == null)
            {
                oldMoviePath = movie.Path;
            }

            foreach (var movieFile in movieFiles)
            {
                var oldMovieFilePath = Path.Combine(oldMoviePath, movieFile.RelativePath);
                movieFile.Path = oldMovieFilePath;

                try
                {
                    _logger.Debug("Renaming movie file: {0}", movieFile);
                    _movieFileMover.MoveMovieFile(movieFile, movie);

                    _mediaFileService.Update(movieFile);
                    _movieService.UpdateMovie(movie);
                    renamed.Add(movieFile);

                    _logger.Debug("Renamed movie file: {0}", movieFile);

                    _eventAggregator.PublishEvent(new MovieFileRenamedEvent(movie, movieFile, oldMovieFilePath));
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to rename file: {0}", oldMovieFilePath);
                }
            }

            if (renamed.Any())
            {
                _eventAggregator.PublishEvent(new MovieRenamedEvent(movie));
            }
        }

        public void RenameMoviePath(Movie movie, bool shouldRenameFiles = true)
        {
            var newFolder = _filenameBuilder.BuildMoviePath(movie);
            if (newFolder != movie.Path && movie.PathState == MoviePathState.Dynamic)
            {
                if (!_configService.AutoRenameFolders)
                {
                    _logger.Info("{0}'s movie should be {1} according to your naming config.", movie, newFolder);
                    return;
                }

                _logger.Info("{0}'s movie folder changed to: {1}", movie, newFolder);
                var oldFolder = movie.Path;
                movie.Path = newFolder;

                _diskProvider.MoveFolder(oldFolder, movie.Path);

                // if (false)
                // {
                //  var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id);
                //  _logger.ProgressInfo("Renaming movie files for {0}", movie.Title);
                //  RenameFiles(movieFiles, movie, oldFolder);
                //  _logger.ProgressInfo("All movie files renamed for {0}", movie.Title);
                // }
                _movieService.UpdateMovie(movie);

                if (_diskProvider.GetFiles(oldFolder, SearchOption.AllDirectories).Count() == 0)
                {
                    _recycleBinProvider.DeleteFolder(oldFolder);
                }
            }

            if (movie.PathState == MoviePathState.StaticOnce)
            {
                movie.PathState = MoviePathState.Dynamic;
                _movieService.UpdateMovie(movie);
            }
        }

        public void Execute(RenameFilesCommand message)
        {
            var movie = _movieService.GetMovie(message.MovieId);
            var movieFiles = _mediaFileService.GetMovies(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", movieFiles.Count, movie.Title);
            RenameFiles(movieFiles, movie);
            _logger.ProgressInfo("Selected movie files renamed for {0}", movie.Title);
        }

        public void Execute(RenameMovieCommand message)
        {
            _logger.Debug("Renaming movie files for selected movie");
            var moviesToRename = _movieService.GetMovies(message.MovieIds);

            foreach (var movie in moviesToRename)
            {
                var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id);
                _logger.ProgressInfo("Renaming movie files for {0}", movie.Title);
                RenameFiles(movieFiles, movie);
                _logger.ProgressInfo("All movie files renamed for {0}", movie.Title);
            }
        }

        public void Execute(RenameMovieFolderCommand message)
        {
            try
            {
                _logger.Debug("Renaming movie folder for selected movie if necessary");
                var moviesToRename = _movieService.GetMovies(message.MovieIds);
                foreach (var movie in moviesToRename)
                {
                    var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id);

                    //_logger.ProgressInfo("Renaming movie folder for {0}", movie.Title);
                    RenameMoviePath(movie);
                }
            }
            catch (SQLiteException ex)
            {
                _logger.Warn(ex, "wtf: {0}, {1}", ex.ResultCode, ex.Data);
            }
        }
    }
}
