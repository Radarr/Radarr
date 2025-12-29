using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.MusicStats
{
    public interface IMusicStatisticsService
    {
        List<MusicStatistics> AlbumStatistics();
        MusicStatistics AlbumStatistics(int albumId);
    }

    public class MusicStatisticsService : IMusicStatisticsService
    {
        private readonly IMusicStatisticsRepository _musicStatisticsRepository;

        public MusicStatisticsService(IMusicStatisticsRepository musicStatisticsRepository)
        {
            _musicStatisticsRepository = musicStatisticsRepository;
        }

        public List<MusicStatistics> AlbumStatistics()
        {
            var albumStatistics = _musicStatisticsRepository.AlbumStatistics();

            return albumStatistics.GroupBy(a => a.AlbumId).Select(a => a.First()).ToList();
        }

        public MusicStatistics AlbumStatistics(int albumId)
        {
            var stats = _musicStatisticsRepository.AlbumStatistics(albumId);

            if (stats == null || stats.Count == 0)
            {
                return new MusicStatistics();
            }

            return stats[0];
        }
    }
}
