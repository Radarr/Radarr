using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface ITVShowRepository : IBasicRepository<TVShow>
    {
        bool TVShowPathExists(string path);
        TVShow FindByTvdbId(int tvdbId);
        TVShow FindByImdbId(string imdbId);
        TVShow FindByAniDbId(int aniDbId);
        TVShow FindByTitle(string title);
        TVShow FindByPath(string path);
        List<TVShow> GetMonitored();
        Dictionary<int, string> AllTVShowPaths();
        Dictionary<int, List<int>> AllTVShowTags();
    }

    public class TVShowRepository : BasicRepository<TVShow>, ITVShowRepository
    {
        public TVShowRepository(IMainDatabase database,
                                IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool TVShowPathExists(string path)
        {
            return Query(s => s.Path == path).Any();
        }

        public TVShow FindByTvdbId(int tvdbId)
        {
            return Query(s => s.TvdbId == tvdbId).FirstOrDefault();
        }

        public TVShow FindByImdbId(string imdbId)
        {
            return Query(s => s.ImdbId == imdbId).FirstOrDefault();
        }

        public TVShow FindByAniDbId(int aniDbId)
        {
            return Query(s => s.AniDbId == aniDbId).FirstOrDefault();
        }

        public TVShow FindByTitle(string title)
        {
            return Query(s => s.Title == title).FirstOrDefault();
        }

        public TVShow FindByPath(string path)
        {
            return Query(s => s.Path == path).FirstOrDefault();
        }

        public List<TVShow> GetMonitored()
        {
            return Query(s => s.Monitored);
        }

        public Dictionary<int, string> AllTVShowPaths()
        {
            var tvShows = All();
            return tvShows.ToDictionary(s => s.Id, s => s.Path);
        }

        public Dictionary<int, List<int>> AllTVShowTags()
        {
            var tvShows = All();
            return tvShows.ToDictionary(s => s.Id, s => s.Tags.ToList());
        }
    }
}
