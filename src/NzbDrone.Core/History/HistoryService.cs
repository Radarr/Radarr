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
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        QualityModel GetBestQualityInHistory(Profile profile, int movieId);
        PagingSpec<MovieHistory> Paged(PagingSpec<MovieHistory> pagingSpec);
        MovieHistory MostRecentForMovie(int movieId);
        MovieHistory MostRecentForDownloadId(string downloadId);
        MovieHistory Get(int historyId);
        List<MovieHistory> Find(string downloadId, MovieHistoryEventType eventType);
        List<MovieHistory> FindByDownloadId(string downloadId);
        List<MovieHistory> GetByMovieId(int movieId, MovieHistoryEventType? eventType);
        void UpdateMany(List<MovieHistory> toUpdate);
        string FindDownloadId(MovieImportedEvent trackedDownload);
        List<MovieHistory> Since(DateTime date, MovieHistoryEventType? eventType);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<MovieGrabbedEvent>,
                                  IHandle<MovieImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<MovieFileDeletedEvent>,
                                  IHandle<MovieFileRenamedEvent>,
                                  IHandle<MoviesDeletedEvent>,
                                  IHandle<DownloadIgnoredEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<MovieHistory> Paged(PagingSpec<MovieHistory> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public MovieHistory MostRecentForMovie(int movieId)
        {
            return _historyRepository.MostRecentForMovie(movieId);
        }

        public MovieHistory MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public MovieHistory Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<MovieHistory> Find(string downloadId, MovieHistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<MovieHistory> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public List<MovieHistory> GetByMovieId(int movieId, MovieHistoryEventType? eventType)
        {
            return _historyRepository.GetByMovieId(movieId, eventType);
        }

        public QualityModel GetBestQualityInHistory(Profile profile, int movieId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestQualityInHistory(movieId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        public void UpdateMany(List<MovieHistory> toUpdate)
        {
            _historyRepository.UpdateMany(toUpdate);
        }

        public string FindDownloadId(MovieImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedMovie.Path);

            var movieId = trackedDownload.MovieInfo.Movie.Id;
            var movieHistory = _historyRepository.FindDownloadHistory(movieId, trackedDownload.ImportedMovie.Quality);

            var processedDownloadId = movieHistory
                .Where(c => c.EventType != MovieHistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = movieHistory.Where(c => c.EventType == MovieHistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                if (stillDownloading.Count != 1)
                {
                    return null;
                }

                downloadId = stillDownloading.Single().DownloadId;
            }

            return downloadId;
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.Grabbed,
                Date = DateTime.UtcNow,
                Quality = message.Movie.ParsedMovieInfo.Quality,
                Languages = message.Movie.ParsedMovieInfo.Languages,
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
            history.Data.Add("DownloadClientName", message.DownloadClientName);
            history.Data.Add("Size", message.Movie.Release.Size.ToString());
            history.Data.Add("DownloadUrl", message.Movie.Release.DownloadUrl);
            history.Data.Add("Guid", message.Movie.Release.Guid);
            history.Data.Add("TmdbId", message.Movie.Release.TmdbId.ToString());
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
            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.DownloadFolderImported,
                Date = DateTime.UtcNow,
                Quality = message.MovieInfo.Quality,
                Languages = message.MovieInfo.Languages,
                SourceTitle = message.ImportedMovie.SceneName ?? Path.GetFileNameWithoutExtension(message.MovieInfo.Path),
                DownloadId = downloadId,
                MovieId = movie.Id,
            };

            history.Data.Add("FileId", message.ImportedMovie.Id.ToString());
            history.Data.Add("DroppedPath", message.MovieInfo.Path);
            history.Data.Add("ImportedPath", Path.Combine(movie.Path, message.ImportedMovie.RelativePath));
            history.Data.Add("DownloadClient", message.DownloadClientInfo?.Type);
            history.Data.Add("DownloadClientName", message.DownloadClientInfo?.Name);
            history.Data.Add("ReleaseGroup", message.MovieInfo.ReleaseGroup);

            _historyRepository.Insert(history);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing movie file from DB as part of cleanup routine, not creating history event.");
                return;
            }

            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.MovieFileDeleted,
                Date = DateTime.UtcNow,
                Quality = message.MovieFile.Quality,
                Languages = message.MovieFile.Languages,
                SourceTitle = message.MovieFile.Path,
                MovieId = message.MovieFile.MovieId
            };

            history.Data.Add("Reason", message.Reason.ToString());
            history.Data.Add("ReleaseGroup", message.MovieFile.ReleaseGroup);

            _historyRepository.Insert(history);
        }

        public void Handle(MovieFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var sourceRelativePath = message.Movie.Path.GetRelativePath(message.OriginalPath);
            var path = Path.Combine(message.Movie.Path, message.MovieFile.RelativePath);
            var relativePath = message.MovieFile.RelativePath;

            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.MovieFileRenamed,
                Date = DateTime.UtcNow,
                Quality = message.MovieFile.Quality,
                Languages = message.MovieFile.Languages,
                SourceTitle = message.OriginalPath,
                MovieId = message.MovieFile.MovieId,
            };

            history.Data.Add("SourcePath", sourcePath);
            history.Data.Add("SourceRelativePath", sourceRelativePath);
            history.Data.Add("Path", path);
            history.Data.Add("RelativePath", relativePath);
            history.Data.Add("ReleaseGroup", message.MovieFile.ReleaseGroup);

            _historyRepository.Insert(history);
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.DownloadIgnored,
                Date = DateTime.UtcNow,
                Quality = message.Quality,
                SourceTitle = message.SourceTitle,
                MovieId = message.MovieId,
                DownloadId = message.DownloadId,
                Languages = message.Languages
            };

            history.Data.Add("DownloadClient", message.DownloadClientInfo.Type);
            history.Data.Add("DownloadClientName", message.DownloadClientInfo.Name);
            history.Data.Add("Message", message.Message);
            history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteMovie?.ParsedMovieInfo?.ReleaseGroup);

            _historyRepository.Insert(history);
        }

        public void Handle(MoviesDeletedEvent message)
        {
            _historyRepository.DeleteForMovies(message.Movies.Select(m => m.Id).ToList());
        }

        public void Handle(DownloadFailedEvent message)
        {
            var history = new MovieHistory
            {
                EventType = MovieHistoryEventType.DownloadFailed,
                Date = DateTime.UtcNow,
                Quality = message.Quality,
                Languages = message.Languages,
                SourceTitle = message.SourceTitle,
                MovieId = message.MovieId,
                DownloadId = message.DownloadId
            };

            history.Data.Add("DownloadClient", message.DownloadClient);
            history.Data.Add("DownloadClientName", message.TrackedDownload?.DownloadItem.DownloadClientInfo.Name);
            history.Data.Add("Message", message.Message);
            history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteMovie?.ParsedMovieInfo?.ReleaseGroup);

            _historyRepository.Insert(history);
        }

        public List<MovieHistory> Since(DateTime date, MovieHistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }
    }
}
