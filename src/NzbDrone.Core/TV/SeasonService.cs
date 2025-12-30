using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.TV.Events;

namespace NzbDrone.Core.TV
{
    public interface ISeasonService
    {
        Season GetSeason(int seasonId);
        List<Season> GetSeasons(IEnumerable<int> seasonIds);
        List<Season> GetSeasonsByTVShowId(int tvShowId);
        Season GetSeasonByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        Season AddSeason(Season newSeason);
        void DeleteSeason(int seasonId);
        Season UpdateSeason(Season season);
        List<Season> UpdateSeasons(List<Season> seasons);
    }

    public class SeasonService : ISeasonService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly IHierarchicalMonitoringService _hierarchicalMonitoringService;
        private readonly IEventAggregator _eventAggregator;

        public SeasonService(
            ISeasonRepository seasonRepository,
            IHierarchicalMonitoringService hierarchicalMonitoringService,
            IEventAggregator eventAggregator)
        {
            _seasonRepository = seasonRepository;
            _hierarchicalMonitoringService = hierarchicalMonitoringService;
            _eventAggregator = eventAggregator;
        }

        public Season GetSeason(int seasonId)
        {
            return _seasonRepository.Get(seasonId);
        }

        public List<Season> GetSeasons(IEnumerable<int> seasonIds)
        {
            return _seasonRepository.Get(seasonIds).ToList();
        }

        public List<Season> GetSeasonsByTVShowId(int tvShowId)
        {
            return _seasonRepository.FindByTVShowId(tvShowId);
        }

        public Season GetSeasonByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
        {
            return _seasonRepository.FindByTVShowIdAndSeasonNumber(tvShowId, seasonNumber);
        }

        public Season AddSeason(Season newSeason)
        {
            var season = _seasonRepository.Insert(newSeason);
            _eventAggregator.PublishEvent(new SeasonAddedEvent(season));
            return season;
        }

        public void DeleteSeason(int seasonId)
        {
            var season = _seasonRepository.Get(seasonId);
            _seasonRepository.Delete(seasonId);
            _eventAggregator.PublishEvent(new SeasonDeletedEvent(season));
        }

        public Season UpdateSeason(Season season)
        {
            var existingSeason = _seasonRepository.Get(season.Id);

            if (existingSeason.Monitored != season.Monitored)
            {
                _hierarchicalMonitoringService.SetSeasonMonitored(season.Id, season.Monitored);
            }

            var updatedSeason = _seasonRepository.Update(season);
            _eventAggregator.PublishEvent(new SeasonEditedEvent(updatedSeason, existingSeason));
            return updatedSeason;
        }

        public List<Season> UpdateSeasons(List<Season> seasons)
        {
            _seasonRepository.UpdateMany(seasons);
            return seasons;
        }
    }
}
