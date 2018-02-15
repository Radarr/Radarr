using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using Marr.Data;

namespace NzbDrone.Core.Extras
{
    public interface IExtraService
    {
        void ImportExtraFiles(LocalEpisode localEpisode, EpisodeFile episodeFile, bool isReadOnly);
        void ImportExtraFiles(LocalMovie localMovie, MovieFile movieFile, bool isReadOnly);
    }

    public class ExtraService : IExtraService,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<EpisodeFolderCreatedEvent>,
                                IHandle<SeriesRenamedEvent>,
                                IHandle<MovieFolderCreatedEvent>,
                                IHandle<MovieRenamedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IEpisodeService _episodeService;
        private readonly IMovieService _movieService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly List<IManageExtraFiles> _extraFileManagers;
        private readonly Logger _logger;

        public ExtraService(IMediaFileService mediaFileService,
                            IEpisodeService episodeService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            List<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _episodeService = episodeService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public ExtraService(IMediaFileService mediaFileService,
                            IMovieService movieService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            List<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _movieService = movieService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public void ImportExtraFiles(LocalEpisode localEpisode, EpisodeFile episodeFile, bool isReadOnly)
        {
            var series = localEpisode.Series;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeImport(series, episodeFile);
            }

            var sourcePath = localEpisode.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var files = _diskProvider.GetFiles(sourceFolder, SearchOption.TopDirectoryOnly);

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.'))
                                                                     .ToList();

            var matchingFilenames = files.Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(sourceFileName));

            foreach (var matchingFilename in matchingFilenames)
            {
                var matchingExtension = wantedExtensions.FirstOrDefault(e => matchingFilename.EndsWith(e));

                if (matchingExtension == null)
                {
                    continue;
                }

                try
                {
                    foreach (var extraFileManager in _extraFileManagers)
                    {
                        var extension = Path.GetExtension(matchingFilename);
                        var extraFile = extraFileManager.Import(series, episodeFile, matchingFilename, extension, isReadOnly);

                        if (extraFile != null)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", matchingFilename);
                }
            }
        }

        public void ImportExtraFiles(LocalMovie localMovie, MovieFile movieFile, bool isReadOnly)
        {
            var movie = localMovie.Movie;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieImport(movie, movieFile);
            }

            if (!_configService.ImportExtraFiles)
            {
                return;
            }

            var sourcePath = localMovie.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var files = _diskProvider.GetFiles(sourceFolder, SearchOption.TopDirectoryOnly);

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.'))
                                                                     .ToList();

            var matchingFilenames = files.Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(sourceFileName, StringComparison.InvariantCultureIgnoreCase));

            foreach (var matchingFilename in matchingFilenames)
            {
                var matchingExtension = wantedExtensions.FirstOrDefault(e => matchingFilename.EndsWith(e));

                if (matchingExtension == null)
                {
                    continue;
                }

                try
                {
                    foreach (var extraFileManager in _extraFileManagers)
                    {
                        var extraFile = extraFileManager.Import(movie, movieFile, matchingFilename, matchingExtension, isReadOnly);

                        if (extraFile != null)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", matchingFilename);
                }
            }
        }

        //public void Handle(MediaCoversUpdatedEvent message)
        //{
        //    var series = message.Series;
        //    var episodeFiles = GetEpisodeFiles(series.Id);

        //    foreach (var extraFileManager in _extraFileManagers)
        //    {
        //        extraFileManager.CreateAfterSeriesScan(series, episodeFiles);
        //    }
        //}

        public void Handle(MediaCoversUpdatedEvent message)
        {
            var movie = message.Movie;
            var movieFiles = GetMovieFiles(movie.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieScan(movie, movieFiles);
            }
        }

        public void Handle(EpisodeFolderCreatedEvent message)
        {
            var series = message.Series;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeImport(series, message.SeriesFolder, message.SeasonFolder);
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            var series = message.Series;
            var episodeFiles = GetEpisodeFiles(series.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(series, episodeFiles);
            }
        }

        public void Handle(MovieFolderCreatedEvent message)
        {
            var movie = message.Movie;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterMovieImport(movie, message.MovieFolder);
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

        private List<EpisodeFile> GetEpisodeFiles(int seriesId)
        {
            var episodeFiles = _mediaFileService.GetFilesBySeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);

            foreach (var episodeFile in episodeFiles)
            {
                var localEpisodeFile = episodeFile;
                episodeFile.Episodes = new LazyList<Episode>(episodes.Where(e => e.EpisodeFileId == localEpisodeFile.Id));
            }

            return episodeFiles;
        }

        private List<MovieFile> GetMovieFiles(int movieId)
        {
            var movieFiles = _mediaFileService.GetFilesByMovie(movieId);

            foreach (var movieFile in movieFiles)
            {
                var localMovieFile = movieFile;
                movieFile.Movie = new LazyLoaded<Movie>(_movieService.GetMovie(movieId));
            }

            return movieFiles;
        }
    }
}
