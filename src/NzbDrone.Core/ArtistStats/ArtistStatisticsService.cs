using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.ArtistStats
{
    public interface IArtistStatisticsService
    {
        List<ArtistStatistics> ArtistStatistics();
        ArtistStatistics ArtistStatistics(int artistId);
    }

    public class ArtistStatisticsService : IArtistStatisticsService,
        IHandle<ArtistUpdatedEvent>,
        IHandle<ArtistDeletedEvent>,
        IHandle<AlbumImportedEvent>,
        IHandle<AlbumEditedEvent>,
        IHandle<TrackFileDeletedEvent>
    {
        private readonly IArtistStatisticsRepository _artistStatisticsRepository;
        private readonly ICached<List<AlbumStatistics>> _cache;

        public ArtistStatisticsService(IArtistStatisticsRepository artistStatisticsRepository,
                                       ICacheManager cacheManager)
        {
            _artistStatisticsRepository = artistStatisticsRepository;
            _cache = cacheManager.GetCache<List<AlbumStatistics>>(GetType());
        }

        public List<ArtistStatistics> ArtistStatistics()
        {
            var albumStatistics = _cache.Get("AllArtists", () => _artistStatisticsRepository.ArtistStatistics());

            return albumStatistics.GroupBy(s => s.ArtistId).Select(s => MapArtistStatistics(s.ToList())).ToList();
        }

        public ArtistStatistics ArtistStatistics(int artistId)
        {
            var stats = _cache.Get(artistId.ToString(), () => _artistStatisticsRepository.ArtistStatistics(artistId));

            if (stats == null || stats.Count == 0) return new ArtistStatistics();

            return MapArtistStatistics(stats);
        }

        private ArtistStatistics MapArtistStatistics(List<AlbumStatistics> albumStatistics)
        {
            var artistStatistics = new ArtistStatistics
                                   {
                                       AlbumStatistics = albumStatistics,
                                       AlbumCount = albumStatistics.Count,
                                       ArtistId = albumStatistics.First().ArtistId,
                                       TrackFileCount = albumStatistics.Sum(s => s.TrackFileCount),
                                       TrackCount = albumStatistics.Sum(s => s.TrackCount),
                                       TotalTrackCount = albumStatistics.Sum(s => s.TotalTrackCount),
                                       SizeOnDisk = albumStatistics.Sum(s => s.SizeOnDisk)
                                   };

            return artistStatistics;
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(ArtistUpdatedEvent message)
        {
            _cache.Remove("AllArtists");
            _cache.Remove(message.Artist.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(ArtistDeletedEvent message)
        {
            _cache.Remove("AllArtists");
            _cache.Remove(message.Artist.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AlbumImportedEvent message)
        {
            _cache.Remove("AllArtists");
            _cache.Remove(message.Artist.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AlbumEditedEvent message)
        {
            _cache.Remove("AllArtists");
            _cache.Remove(message.Album.ArtistId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(TrackFileDeletedEvent message)
        {
            _cache.Remove("AllArtists");
            _cache.Remove(message.TrackFile.Artist.Value.Id.ToString());
        }
    }
}
