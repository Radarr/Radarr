using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.ArtistStats
{
    public interface IArtistStatisticsService
    {
        List<ArtistStatistics> ArtistStatistics();
        ArtistStatistics ArtistStatistics(int artistId);
    }

    public class ArtistStatisticsService : IArtistStatisticsService
    {
        private readonly IArtistStatisticsRepository _artistStatisticsRepository;

        public ArtistStatisticsService(IArtistStatisticsRepository artistStatisticsRepository)
        {
            _artistStatisticsRepository = artistStatisticsRepository;
        }

        public List<ArtistStatistics> ArtistStatistics()
        {
            var albumStatistics = _artistStatisticsRepository.ArtistStatistics();

            return albumStatistics.GroupBy(s => s.ArtistId).Select(s => MapArtistStatistics(s.ToList())).ToList();
        }

        public ArtistStatistics ArtistStatistics(int artistId)
        {
            var stats = _artistStatisticsRepository.ArtistStatistics(artistId);

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
    }
}
