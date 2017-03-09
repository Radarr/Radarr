using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        QualityModel GetBestQualityInHistory(Profile profile, int episodeId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForMovie(int movieId);
        History MostRecentForEpisode(int episodeId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<EpisodeGrabbedEvent>,
                                  IHandle<MovieGrabbedEvent>,
                                  IHandle<MovieImportedEvent>,
                                  IHandle<EpisodeImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<EpisodeFileDeletedEvent>,
                                  IHandle<MovieFileDeletedEvent>,
                                  IHandle<SeriesDeletedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<History> Paged(PagingSpec<History> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public History MostRecentForEpisode(int episodeId)
        {
            return _historyRepository.MostRecentForEpisode(episodeId);
        }

        public History MostRecentForMovie(int movieId)
        {
            return _historyRepository.MostRecentForMovie(movieId);
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public History Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<History> Find(string downloadId, HistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public QualityModel GetBestQualityInHistory(Profile profile, int episodeId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestQualityInHistory(episodeId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var history = new History
            {
                EventType = HistoryEventType.Grabbed,
                Date = DateTime.UtcNow,
                Quality = message.Movie.ParsedMovieInfo.Quality,
                SourceTitle = message.Movie.Release.Title,
                SeriesId = 0,
                EpisodeId = 0,
                DownloadId = message.DownloadId,
                MovieId = message.Movie.Movie.Id
            };

            history.Data.Add("Indexer", message.Movie.Release.Indexer);
            history.Data.Add("NzbInfoUrl", message.Movie.Release.InfoUrl);
            history.Data.Add("ReleaseGroup", message.Movie.ParsedMovieInfo.ReleaseGroup);
            history.Data.Add("Age", message.Movie.Release.Age.ToString());
            history.Data.Add("AgeHours", message.Movie.Release.AgeHours.ToString());
            history.Data.Add("AgeMinutes", message.Movie.Release.AgeMinutes.ToString());
            history.Data.Add("PublishedDate", message.Movie.Release.PublishDate.ToString("s") + "Z");
            history.Data.Add("DownloadClient", message.DownloadClient);
            history.Data.Add("Size", message.Movie.Release.Size.ToString());
            history.Data.Add("DownloadUrl", message.Movie.Release.DownloadUrl);
            history.Data.Add("Guid", message.Movie.Release.Guid);
            history.Data.Add("TvdbId", message.Movie.Release.TvdbId.ToString());
            history.Data.Add("TvRageId", message.Movie.Release.TvRageId.ToString());
            history.Data.Add("Protocol", ((int)message.Movie.Release.DownloadProtocol).ToString());

            if (!message.Movie.ParsedMovieInfo.ReleaseHash.IsNullOrWhiteSpace())
            {
                history.Data.Add("ReleaseHash", message.Movie.ParsedMovieInfo.ReleaseHash);
            }

            var torrentRelease = message.Movie.Release as TorrentInfo;

            if (torrentRelease != null)
            {
                history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
            }

            _historyRepository.Insert(history);
        }

        public void Handle(MovieImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = FindDownloadId(message); //For now fuck off.
            }

            var movie = message.MovieInfo.Movie;
            var history = new History
            {
                EventType = HistoryEventType.DownloadFolderImported,
                Date = DateTime.UtcNow,
                Quality = message.MovieInfo.Quality,
                SourceTitle = movie.Title,
                SeriesId = 0,
                EpisodeId = 0,
                DownloadId = downloadId,
                MovieId = movie.Id,
            };

            //Won't have a value since we publish this event before saving to DB.
            //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
            history.Data.Add("DroppedPath", message.MovieInfo.Path);
            history.Data.Add("ImportedPath", Path.Combine(movie.Path, message.ImportedMovie.RelativePath));
            history.Data.Add("DownloadClient", message.DownloadClient);

            _historyRepository.Insert(history);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing movie file from DB as part of cleanup routine, not creating history event.");
                return;
            }

            var history = new History
            {
                EventType = HistoryEventType.MovieFileDeleted,
                Date = DateTime.UtcNow,
                Quality = message.MovieFile.Quality,
                SourceTitle = message.MovieFile.Path,
                SeriesId = 0,
                EpisodeId = 0,
                MovieId = message.MovieFile.MovieId
            };

            history.Data.Add("Reason", message.Reason.ToString());

            _historyRepository.Insert(history);
        }

        public void Handle(MovieDeletedEvent message)
        {
            _historyRepository.DeleteForMovie(message.Movie.Id);
        }

        private string FindDownloadId(MovieImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedMovie.Path);

            var movieId = trackedDownload.MovieInfo.Movie.Id;

            var allHistory = _historyRepository.FindDownloadHistory(movieId, trackedDownload.ImportedMovie.Quality);

            //Find download related items for this movie
            var movieHistory = allHistory.Where(h => movieId == h.MovieId).ToList();

            var processedDownloadId = movieHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = movieHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                //foreach (var matchingHistory in trackedDownload.EpisodeInfo.Episodes.Select(e => stillDownloading.Where(c => c.MovieId == e.Id).ToList()))
                //foreach (var matchingHistory in stillDownloading.Where(c => c.MovieId == e.Id).ToList())
                //{
                    if (stillDownloading.Count != 1)
                    {
                        return null;
                    }

                    var newDownloadId = stillDownloading.Single().DownloadId;

                    if (downloadId == null || downloadId == newDownloadId)
                    {
                        downloadId = newDownloadId;
                    }
                    else
                    {
                        return null;
                    }
                //}
            }

            return downloadId;
        }

        private string FindDownloadId(EpisodeImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedEpisode.Path);

            var episodeIds = trackedDownload.EpisodeInfo.Episodes.Select(c => c.Id).ToList();

            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.EpisodeInfo.Series.Id, trackedDownload.ImportedEpisode.Quality);


            //Find download related items for these episdoes
            var episodesHistory = allHistory.Where(h => episodeIds.Contains(h.EpisodeId)).ToList();

            var processedDownloadId = episodesHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = episodesHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.EpisodeInfo.Episodes.Select(e => stillDownloading.Where(c => c.EpisodeId == e.Id).ToList()))
                {
                    if (matchingHistory.Count != 1)
                    {
                        return null;
                    }

                    var newDownloadId = matchingHistory.Single().DownloadId;

                    if (downloadId == null || downloadId == newDownloadId)
                    {
                        downloadId = newDownloadId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return downloadId;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var history = new History
                {
                    EventType = HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Episode.ParsedEpisodeInfo.Quality,
                    SourceTitle = message.Episode.Release.Title,
                    SeriesId = episode.SeriesId,
                    EpisodeId = episode.Id,
                    DownloadId = message.DownloadId,
                    MovieId = 0
                };

                history.Data.Add("Indexer", message.Episode.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Episode.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Episode.ParsedEpisodeInfo.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Episode.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Episode.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Episode.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Size", message.Episode.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Episode.Release.DownloadUrl);
                history.Data.Add("Guid", message.Episode.Release.Guid);
                history.Data.Add("TvdbId", message.Episode.Release.TvdbId.ToString());
                history.Data.Add("TvRageId", message.Episode.Release.TvRageId.ToString());
                history.Data.Add("Protocol", ((int)message.Episode.Release.DownloadProtocol).ToString());

                if (!message.Episode.ParsedEpisodeInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Episode.ParsedEpisodeInfo.ReleaseHash);
                }

                var torrentRelease = message.Episode.Release as TorrentInfo;

                if (torrentRelease != null)
                {
                    history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
                }

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = FindDownloadId(message);
            }

            foreach (var episode in message.EpisodeInfo.Episodes)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadFolderImported,
                    Date = DateTime.UtcNow,
                    Quality = message.EpisodeInfo.Quality,
                    SourceTitle = message.ImportedEpisode.SceneName ?? Path.GetFileNameWithoutExtension(message.EpisodeInfo.Path),
                    SeriesId = message.ImportedEpisode.SeriesId,
                    EpisodeId = episode.Id,
                    DownloadId = downloadId,
                    MovieId = 0,
                };

                //Won't have a value since we publish this event before saving to DB.
                //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.EpisodeInfo.Path);
                history.Data.Add("ImportedPath", Path.Combine(message.EpisodeInfo.Series.Path, message.ImportedEpisode.RelativePath));
                history.Data.Add("DownloadClient", message.DownloadClient);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadFailedEvent message)
        {
            var history = new History
            {
                EventType = HistoryEventType.DownloadFailed,
                Date = DateTime.UtcNow,
                Quality = message.Quality,
                SourceTitle = message.SourceTitle,
                SeriesId = 0,
                EpisodeId = 0,
                MovieId = message.MovieId,
                DownloadId = message.DownloadId
            };

            history.Data.Add("DownloadClient", message.DownloadClient);
            history.Data.Add("Message", message.Message);

            _historyRepository.Insert(history);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing episode file from DB as part of cleanup routine, not creating history event.");
                return;
            }

            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                var history = new History
                {
                    EventType = HistoryEventType.EpisodeFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.EpisodeFile.Quality,
                    SourceTitle = message.EpisodeFile.Path,
                    SeriesId = message.EpisodeFile.SeriesId,
                    EpisodeId = episode.Id,
                    MovieId = 0
                };

                history.Data.Add("Reason", message.Reason.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _historyRepository.DeleteForSeries(message.Series.Id);
        }
    }
}