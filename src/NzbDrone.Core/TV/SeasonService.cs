using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface ISeasonService
    {
        Season GetSeason(int seasonId);
        List<Season> GetSeasons(IEnumerable<int> seasonIds);
        List<Season> GetSeasonsByTVShowId(int tvShowId);
        Season FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        Season AddSeason(Season newSeason);
        List<Season> AddSeasons(List<Season> newSeasons);
        void DeleteSeason(int seasonId);
        void DeleteSeasons(List<int> seasonIds);
        Season UpdateSeason(Season season);
        List<Season> UpdateSeasons(List<Season> seasons);
        List<Season> GetMonitored();
    }

    public class SeasonService : ISeasonService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public SeasonService(ISeasonRepository seasonRepository,
                             IEventAggregator eventAggregator,
                             Logger logger)
        {
            _seasonRepository = seasonRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Season GetSeason(int seasonId) => _seasonRepository.Get(seasonId);
        public List<Season> GetSeasons(IEnumerable<int> seasonIds) => _seasonRepository.Get(seasonIds).ToList();
        public List<Season> GetSeasonsByTVShowId(int tvShowId) => _seasonRepository.FindByTVShowId(tvShowId);
        public Season FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
            => _seasonRepository.FindByTVShowIdAndSeasonNumber(tvShowId, seasonNumber);
        public List<Season> GetMonitored() => _seasonRepository.GetMonitored();

        public Season AddSeason(Season newSeason)
        {
            return _seasonRepository.Insert(newSeason);
        }

        public List<Season> AddSeasons(List<Season> newSeasons)
        {
            _seasonRepository.InsertMany(newSeasons);
            return newSeasons;
        }

        public void DeleteSeason(int seasonId)
        {
            _seasonRepository.Delete(seasonId);
        }

        public void DeleteSeasons(List<int> seasonIds)
        {
            _seasonRepository.DeleteMany(seasonIds);
        }

        public Season UpdateSeason(Season season)
        {
            _seasonRepository.Update(season);
            return season;
        }

        public List<Season> UpdateSeasons(List<Season> seasons)
        {
            _seasonRepository.UpdateMany(seasons);
            return seasons;
        }
    }
}
