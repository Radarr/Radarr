using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.AudiobookStats
{
    public interface IAudiobookStatisticsService
    {
        List<AudiobookStatistics> AudiobookStatistics();
        AudiobookStatistics AudiobookStatistics(int audiobookId);
    }

    public class AudiobookStatisticsService : IAudiobookStatisticsService
    {
        private readonly IAudiobookStatisticsRepository _audiobookStatisticsRepository;

        public AudiobookStatisticsService(IAudiobookStatisticsRepository audiobookStatisticsRepository)
        {
            _audiobookStatisticsRepository = audiobookStatisticsRepository;
        }

        public List<AudiobookStatistics> AudiobookStatistics()
        {
            var audiobookStatistics = _audiobookStatisticsRepository.AudiobookStatistics();

            return audiobookStatistics.GroupBy(a => a.AudiobookId).Select(a => a.First()).ToList();
        }

        public AudiobookStatistics AudiobookStatistics(int audiobookId)
        {
            var stats = _audiobookStatisticsRepository.AudiobookStatistics(audiobookId);

            if (stats == null || stats.Count == 0)
            {
                return new AudiobookStatistics();
            }

            return stats.First();
        }
    }
}
