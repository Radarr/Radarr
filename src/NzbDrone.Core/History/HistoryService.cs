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
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        QualityModel GetBestQualityInHistory(Profile profile, int episodeId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForAlbum(int episodeId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<AlbumGrabbedEvent>,
                                  IHandle<EpisodeImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<EpisodeFileDeletedEvent>,
                                  IHandle<ArtistDeletedEvent>
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

        public History MostRecentForAlbum(int albumId)
        {
            return _historyRepository.MostRecentForAlbum(albumId);
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

        public QualityModel GetBestQualityInHistory(Profile profile, int albumId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestQualityInHistory(albumId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        private string FindDownloadId(EpisodeImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedEpisode.Path);

            var albumIds = trackedDownload.EpisodeInfo.Episodes.Select(c => c.Id).ToList();

            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.EpisodeInfo.Series.Id, trackedDownload.ImportedEpisode.Quality);


            //Find download related items for these episdoes
            var albumsHistory = allHistory.Where(h => albumIds.Contains(h.AlbumId)).ToList();

            var processedDownloadId = albumsHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = albumsHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.EpisodeInfo.Episodes.Select(e => stillDownloading.Where(c => c.AlbumId == e.Id).ToList()))
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

        public void Handle(AlbumGrabbedEvent message)
        {
            foreach (var album in message.Album.Albums)
            {
                var history = new History
                {
                    EventType = HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Album.ParsedAlbumInfo.Quality,
                    SourceTitle = message.Album.Release.Title,
                    ArtistId = album.ArtistId,
                    AlbumId = album.Id,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("Indexer", message.Album.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Album.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Album.ParsedAlbumInfo.ReleaseGroup);
                history.Data.Add("Age", message.Album.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Album.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Album.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Album.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Size", message.Album.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Album.Release.DownloadUrl);
                history.Data.Add("Guid", message.Album.Release.Guid);
                history.Data.Add("Protocol", ((int)message.Album.Release.DownloadProtocol).ToString());

                if (!message.Album.ParsedAlbumInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Album.ParsedAlbumInfo.ReleaseHash);
                }

                var torrentRelease = message.Album.Release as TorrentInfo;

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
                        ArtistId = message.ImportedEpisode.SeriesId,
                        AlbumId = episode.Id,
                        DownloadId = downloadId
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
            foreach (var albumId in message.AlbumIds)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    ArtistId = message.SeriesId,
                    AlbumId = albumId,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }
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
                    ArtistId = message.EpisodeFile.SeriesId,
                    AlbumId = episode.Id,
                };

                history.Data.Add("Reason", message.Reason.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(ArtistDeletedEvent message)
        {
            _historyRepository.DeleteForArtist(message.Artist.Id);
        }
    }
}