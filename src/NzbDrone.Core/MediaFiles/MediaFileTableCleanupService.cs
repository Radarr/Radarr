using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Series series, List<string> filesOnDisk);

        void Clean(Movie movie, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IMovieService _movieService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IMovieService movieService,
                                            IEpisodeService episodeService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _movieService = movieService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Clean(Series series, List<string> filesOnDisk)
        {
            var seriesFiles = _mediaFileService.GetFilesBySeries(series.Id);
            var episodes = _episodeService.GetEpisodeBySeries(series.Id);

            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);
            
            foreach (var seriesFile in seriesFiles)
            {
                var episodeFile = seriesFile;
                var episodeFilePath = Path.Combine(series.Path, episodeFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(episodeFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", episodeFilePath);
                        _mediaFileService.Delete(seriesFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }

                    if (episodes.None(e => e.EpisodeFileId == episodeFile.Id))
                    {
                        _logger.Debug("File [{0}] is not assigned to any episodes, removing from db", episodeFilePath);
                        _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.NoLinkedEpisodes);
                        continue;
                    }

//                    var localEpsiode = _parsingService.GetLocalEpisode(episodeFile.Path, series);
//
//                    if (localEpsiode == null || episodes.Count != localEpsiode.Episodes.Count)
//                    {
//                        _logger.Debug("File [{0}] parsed episodes has changed, removing from db", episodeFile.Path);
//                        _mediaFileService.Delete(episodeFile);
//                        continue;
//                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = string.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.Id);
                    _logger.Error(ex, errorMessage);
                }
            }

            foreach (var e in episodes)
            {
                var episode = e;

                if (episode.EpisodeFileId > 0 && seriesFiles.None(f => f.Id == episode.EpisodeFileId))
                {
                    episode.EpisodeFileId = 0;
                    _episodeService.UpdateEpisode(episode);
                }
            }
        }

        public void Clean(Movie movie, List<string> filesOnDisk)
        {
            var movieFiles = _mediaFileService.GetFilesByMovie(movie.Id);
            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);

            foreach (var movieFile in movieFiles)
            {
                var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(movieFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", movieFilePath);
                        _mediaFileService.Delete(movieFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = string.Format("Unable to cleanup MovieFile in DB: {0}", movieFile.Id);
                    _logger.Error(ex, errorMessage);
                }
            }
        }
    }
}