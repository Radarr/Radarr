using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface IEpisodeFileRepository : IBasicRepository<EpisodeFile>
    {
        List<EpisodeFile> FindByTVShowId(int tvShowId);
        List<EpisodeFile> FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        EpisodeFile FindByPath(string path);
    }

    public class EpisodeFileRepository : BasicRepository<EpisodeFile>, IEpisodeFileRepository
    {
        public EpisodeFileRepository(IMainDatabase database,
                                     IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<EpisodeFile> FindByTVShowId(int tvShowId)
        {
            return Query(f => f.TVShowId == tvShowId);
        }

        public List<EpisodeFile> FindByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
        {
            return Query(f => f.TVShowId == tvShowId && f.SeasonNumber == seasonNumber);
        }

        public EpisodeFile FindByPath(string path)
        {
            return Query(f => f.Path == path).FirstOrDefault();
        }
    }
}
