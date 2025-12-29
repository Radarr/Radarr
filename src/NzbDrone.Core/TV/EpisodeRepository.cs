using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        List<Episode> FindByTVShowId(int tvShowId);
        List<Episode> FindBySeasonId(int seasonId);
        Episode FindByTVShowIdAndEpisodeNumber(int tvShowId, int seasonNumber, int episodeNumber);
        Episode FindByTVShowIdAndAbsoluteNumber(int tvShowId, int absoluteNumber);
        List<Episode> FindByAirDate(int tvShowId, DateTime airDate);
        List<Episode> GetMonitored();
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        public EpisodeRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Episode> FindByTVShowId(int tvShowId)
        {
            return Query(e => e.TVShowId == tvShowId);
        }

        public List<Episode> FindBySeasonId(int seasonId)
        {
            return Query(e => e.SeasonId == seasonId);
        }

        public Episode FindByTVShowIdAndEpisodeNumber(int tvShowId, int seasonNumber, int episodeNumber)
        {
            return Query(e => e.TVShowId == tvShowId &&
                             e.SeasonNumber == seasonNumber &&
                             e.EpisodeNumber == episodeNumber).FirstOrDefault();
        }

        public Episode FindByTVShowIdAndAbsoluteNumber(int tvShowId, int absoluteNumber)
        {
            return Query(e => e.TVShowId == tvShowId &&
                             e.AbsoluteEpisodeNumber == absoluteNumber).FirstOrDefault();
        }

        public List<Episode> FindByAirDate(int tvShowId, DateTime airDate)
        {
            return Query(e => e.TVShowId == tvShowId &&
                             e.AirDate.HasValue &&
                             e.AirDate.Value.Date == airDate.Date);
        }

        public List<Episode> GetMonitored()
        {
            return Query(e => e.Monitored);
        }
    }
}
