using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras
{
    public interface IExtraService
    {
        void ImportMovie(LocalMovie localMovie, MovieFile movieFile, bool isReadOnly);
    }

    public class ExtraService : IExtraService,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<MovieFolderCreatedEvent>,
                                IHandle<MovieScannedEvent>,
                                IHandle<MovieRenamedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IMovieService _movieService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly List<IManageExtraFiles> _extraFileManagers;

        public ExtraService(IMediaFileService mediaFileService,
                            IMovieService movieService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            IEnumerable<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _movieService = movieService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
        }

        public void ImportMovie(LocalMovie localMovie, MovieFile movieFile, bool isReadOnly)
        {
            ImportExtraFiles(localMovie, movieFile, isReadOnly);

            CreateAfterMovieImport(localMovie.Movie, movieFile);
        }

        public void ImportExtraFiles(LocalMovie localMovie, MovieFile movieFile, bool isReadOnly)
        {
            if (!_configService.ImportExtraFiles)
            {
                return;
            }

            var folderSearchOption = localMovie.FolderMovieInfo == null
                ? SearchOption.TopDirectoryOnly
                : SearchOption.AllDirectories;

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.')
                                                                     .Insert(0, "."))
                                                                     .ToList();

            var sourceFolder = _diskProvider.GetParentFolder(localMovie.Path);
            var files = _diskProvider.GetFiles(sourceFolder, folderSearchOption);
            var managedFiles = _extraFileManagers.Select((i) => new List<string>()).ToArray();

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var matchingExtension = wantedExtensions.FirstOrDefault(e => e.Equals(extension));

                if (matchingExtension == null)
                {
                    continue;
                }

                for (var i = 0; i < _extraFileManagers.Count; i++)
                {
                    if (_extraFileManagers[i].CanImportFile(localMovie, movieFile, file, extension, isReadOnly))
                    {
                        managedFiles[i].Add(file);
                        break;
                    }
                }
            }

            for (var i = 0; i < _extraFileManagers.Count; i++)
            {
                _extraFileManagers[i].ImportFiles(localMovie, movieFile, managedFiles[i], isReadOnly);
            }
        }

        private void CreateAfterMovieImport(Movie movie, MovieFile movieFile)
        {
            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieImport(movie, movieFile);
            }
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                var movie = message.Movie;

                foreach (var extraFileManager in _extraFileManagers)
                {
                    extraFileManager.CreateAfterMediaCoverUpdate(movie);
                }
            }
        }

        public void Handle(MovieScannedEvent message)
        {
            var movie = message.Movie;
            var movieFiles = GetMovieFiles(movie.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieScan(movie, movieFiles);
            }
        }

        public void Handle(MovieFolderCreatedEvent message)
        {
            var movie = message.Movie;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieFolder(movie, message.MovieFolder);
            }
        }

        public void Handle(MovieRenamedEvent message)
        {
            var movie = message.Movie;
            var movieFiles = GetMovieFiles(movie.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(movie, movieFiles);
            }
        }

        private List<MovieFile> GetMovieFiles(int movieId)
        {
            var movieFiles = _mediaFileService.GetFilesByMovie(movieId);

            foreach (var movieFile in movieFiles)
            {
                movieFile.Movie = _movieService.GetMovie(movieId);
            }

            return movieFiles;
        }
    }
}
