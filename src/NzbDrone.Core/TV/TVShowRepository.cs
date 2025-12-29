using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface ITVShowRepository : IBasicRepository<TVShow>
    {
        TVShow FindByTitle(string title);
        TVShow FindByTvdbId(int tvdbId);
        TVShow FindByImdbId(string imdbId);
        List<TVShow> GetMonitored();
        bool TVShowPathExists(string path);
    }

    public class TVShowRepository : BasicRepository<TVShow>, ITVShowRepository
    {
        public TVShowRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public TVShow FindByTitle(string title)
        {
            return Query(t => t.Title == title).FirstOrDefault();
        }

        public TVShow FindByTvdbId(int tvdbId)
        {
            return Query(t => t.TvdbId == tvdbId).FirstOrDefault();
        }

        public TVShow FindByImdbId(string imdbId)
        {
            return Query(t => t.ImdbId == imdbId).FirstOrDefault();
        }

        public List<TVShow> GetMonitored()
        {
            return Query(t => t.Monitored);
        }

        public bool TVShowPathExists(string path)
        {
            return Query(t => t.Path == path).Any();
        }
    }
}
