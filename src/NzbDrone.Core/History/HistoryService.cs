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
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        QualityModel GetBestQualityInHistory(Profile profile, int movieId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForMovie(int movieId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
        List<History> FindByMovieId(int movieId);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<MovieGrabbedEvent>,
                                  IHandle<MovieImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<MovieFileDeletedEvent>,
                                  IHandle<MovieDeletedEvent>
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

        public List<History> FindByMovieId(int movieId)
        {
            return _historyRepository.FindByMovieId(movieId);
        }

        public QualityModel GetBestQualityInHistory(Profile profile, int movieId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestQualityInHistory(movieId)
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
            history.Data.Add("IndexerFlags", message.Movie.Release.IndexerFlags.ToString());
            history.Data.Add("IndexerId", message.Movie.Release.IndexerId.ToString());

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
                SourceTitle = message.ImportedMovie.SceneName ?? Path.GetFileNameWithoutExtension(message.MovieInfo.Path),
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

            var movieHistory = _historyRepository.FindDownloadHistory(movieId, trackedDownload.ImportedMovie.Quality);

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

        public void Handle(DownloadFailedEvent message)
        {
            var history = new History
            {
                EventType = HistoryEventType.DownloadFailed,
                Date = DateTime.UtcNow,
                Quality = message.Quality,
                SourceTitle = message.SourceTitle,
                MovieId = message.MovieId,
                DownloadId = message.DownloadId
            };

            history.Data.Add("DownloadClient", message.DownloadClient);
            history.Data.Add("Message", message.Message);

            _historyRepository.Insert(history);
        }
    }
}
