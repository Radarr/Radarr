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
        List<Episode> FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        Episode FindByTVShowIdAndEpisode(int tvShowId, int seasonNumber, int episodeNumber);
        Episode FindByTVShowIdAndAbsoluteNumber(int tvShowId, int absoluteNumber);
        List<Episode> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        Episode FindByPath(string path);
        Dictionary<int, string> AllEpisodePaths();
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        public EpisodeRepository(IMainDatabase database,
                                 IEventAggregator eventAggregator)
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

        public List<Episode> FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
        {
            return Query(e => e.TVShowId == tvShowId && e.SeasonNumber == seasonNumber);
        }

        public Episode FindByTVShowIdAndEpisode(int tvShowId, int seasonNumber, int episodeNumber)
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

        public List<Episode> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var query = Query(e => e.AirDateUtc >= start && e.AirDateUtc <= end);

            if (!includeUnmonitored)
            {
                query = query.Where(e => e.Monitored).ToList();
            }

            return query;
        }

        public Episode FindByPath(string path)
        {
            return Query(e => e.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllEpisodePaths()
        {
            var episodes = All();
            return episodes.ToDictionary(e => e.Id, e => e.Path);
        }
    }
}
