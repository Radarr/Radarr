using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface ISeasonRepository : IBasicRepository<Season>
    {
        List<Season> FindByTVShowId(int tvShowId);
        Season FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        List<Season> GetMonitored();
    }

    public class SeasonRepository : BasicRepository<Season>, ISeasonRepository
    {
        public SeasonRepository(IMainDatabase database,
                                IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Season> FindByTVShowId(int tvShowId)
        {
            return Query(s => s.TVShowId == tvShowId);
        }

        public Season FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
        {
            return Query(s => s.TVShowId == tvShowId && s.SeasonNumber == seasonNumber).FirstOrDefault();
        }

        public List<Season> GetMonitored()
        {
            return Query(s => s.Monitored);
        }
    }
}
