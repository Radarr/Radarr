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
using NzbDrone.Core.Languages;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForAlbum(int albumId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> GetByArtist(int artistId, HistoryEventType? eventType);
        List<History> GetByAlbum(int albumId, HistoryEventType? eventType);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
        void UpdateMany(IList<History> items);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<AlbumGrabbedEvent>,
                                  IHandle<AlbumImportIncompleteEvent>,
                                  IHandle<TrackImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<DownloadCompletedEvent>,
                                  IHandle<TrackFileDeletedEvent>,
                                  IHandle<TrackFileRenamedEvent>,
                                  IHandle<TrackFileRetaggedEvent>,
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

        public List<History> GetByArtist(int artistId, HistoryEventType? eventType)
        {
            return _historyRepository.GetByArtist(artistId, eventType);
        }

        public List<History> GetByAlbum(int albumId, HistoryEventType? eventType)
        {
            return _historyRepository.GetByAlbum(albumId, eventType);
        }

        public List<History> Find(string downloadId, HistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        private string FindDownloadId(TrackImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedTrack.Path);

            var albumIds = trackedDownload.TrackInfo.Tracks.Select(c => c.AlbumId).ToList();

            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.TrackInfo.Artist.Id, trackedDownload.ImportedTrack.Quality);


            //Find download related items for these episdoes
            var albumsHistory = allHistory.Where(h => albumIds.Contains(h.AlbumId)).ToList();

            var processedDownloadId = albumsHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = albumsHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.TrackInfo.Tracks.Select(e => stillDownloading.Where(c => c.AlbumId == e.AlbumId).ToList()))
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
                    DownloadId = message.DownloadId,
                    Language = message.Album.ParsedAlbumInfo.Language
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
                history.Data.Add("DownloadForced", (!message.Album.DownloadAllowed).ToString());

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

        public void Handle(AlbumImportIncompleteEvent message)
        {
            foreach (var album in message.TrackedDownload.RemoteAlbum.Albums)
            {
                var history = new History
                {
                    EventType = HistoryEventType.AlbumImportIncomplete,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackedDownload.RemoteAlbum.ParsedAlbumInfo?.Quality ?? new QualityModel(),
                    SourceTitle = message.TrackedDownload.DownloadItem.Title,
                    ArtistId = album.ArtistId,
                    AlbumId = album.Id,
                    DownloadId = message.TrackedDownload.DownloadItem.DownloadId,
                    Language = message.TrackedDownload.RemoteAlbum.ParsedAlbumInfo?.Language ?? Language.English
                };

                history.Data.Add("StatusMessages", message.TrackedDownload.StatusMessages.ToJson());
                _historyRepository.Insert(history);
            }
        }

        public void Handle(TrackImportedEvent message)
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

            foreach (var track in message.TrackInfo.Tracks)
            {
                var history = new History
                    {
                        EventType = HistoryEventType.TrackFileImported,
                        Date = DateTime.UtcNow,
                        Quality = message.TrackInfo.Quality,
                        SourceTitle = message.ImportedTrack.SceneName ?? Path.GetFileNameWithoutExtension(message.TrackInfo.Path),
                        ArtistId = message.TrackInfo.Artist.Id,
                        AlbumId = message.TrackInfo.Album.Id,
                        TrackId = track.Id,
                        DownloadId = downloadId,
                        Language = message.TrackInfo.Language
                };

                //Won't have a value since we publish this event before saving to DB.
                //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.TrackInfo.Path);
                history.Data.Add("ImportedPath", message.ImportedTrack.Path);
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
                    ArtistId = message.ArtistId,
                    AlbumId = albumId,
                    DownloadId = message.DownloadId,
                    Language = message.Language
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadCompletedEvent message)
        {
            foreach (var album in message.TrackedDownload.RemoteAlbum.Albums)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadImported,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackedDownload.RemoteAlbum.ParsedAlbumInfo?.Quality ?? new QualityModel(),
                    SourceTitle = message.TrackedDownload.DownloadItem.Title,
                    ArtistId = album.ArtistId,
                    AlbumId = album.Id,
                    DownloadId = message.TrackedDownload.DownloadItem.DownloadId,
                    Language = message.TrackedDownload.RemoteAlbum.ParsedAlbumInfo?.Language ?? Language.English
                };

                _historyRepository.Insert(history);
            }
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing track file from DB as part of cleanup routine, not creating history event.");
                return;
            }
            else if (message.Reason == DeleteMediaFileReason.ManualOverride)
            {
                _logger.Debug("Removing track file from DB as part of manual override of existing file, not creating history event.");
                return;
            }


            foreach (var track in message.TrackFile.Tracks.Value)
            {
                var history = new History
                {
                    EventType = HistoryEventType.TrackFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackFile.Quality,
                    SourceTitle = message.TrackFile.Path,
                    ArtistId = message.TrackFile.Artist.Value.Id,
                    AlbumId = message.TrackFile.AlbumId,
                    TrackId = track.Id,
                };

                history.Data.Add("Reason", message.Reason.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(TrackFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var sourceRelativePath = message.Artist.Path.GetRelativePath(message.OriginalPath);
            var path = message.TrackFile.Path;

            foreach (var track in message.TrackFile.Tracks.Value)
            {
                var history = new History
                {
                    EventType = HistoryEventType.TrackFileRenamed,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackFile.Quality,
                    SourceTitle = message.OriginalPath,
                    ArtistId = message.TrackFile.Artist.Value.Id,
                    AlbumId = message.TrackFile.AlbumId,
                    TrackId = track.Id,
                };

                history.Data.Add("SourcePath", sourcePath);
                history.Data.Add("SourceRelativePath", sourceRelativePath);
                history.Data.Add("Path", path);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(TrackFileRetaggedEvent message)
        {
            var path = message.TrackFile.Path;

            foreach (var track in message.TrackFile.Tracks.Value)
            {
                var history = new History
                {
                    EventType = HistoryEventType.TrackFileRetagged,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackFile.Quality,
                    SourceTitle = path,
                    ArtistId = message.TrackFile.Artist.Value.Id,
                    AlbumId = message.TrackFile.AlbumId,
                    TrackId = track.Id,
                };
                
                history.Data.Add("TagsScrubbed", message.Scrubbed.ToString());
                history.Data.Add("Diff", message.Diff.Select(x => new {
                            Field = x.Key,
                            OldValue = x.Value.Item1,
                            NewValue = x.Value.Item2
                        }).ToJson());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(ArtistDeletedEvent message)
        {
            _historyRepository.DeleteForArtist(message.Artist.Id);
        }

        public List<History> Since(DateTime date, HistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }

        public void UpdateMany(IList<History> items)
        {
            _historyRepository.UpdateMany(items);
        }
    }
}
