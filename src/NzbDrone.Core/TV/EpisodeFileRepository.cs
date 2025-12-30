using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TV
{
    public interface IEpisodeFileRepository : IBasicRepository<EpisodeFile>
    {
        List<EpisodeFile> FindByTVShowId(int tvShowId);
        List<EpisodeFile> FindBySeasonId(int seasonId);
        EpisodeFile FindByEpisodeId(int episodeId);
    }

    public class EpisodeFileRepository : BasicRepository<EpisodeFile>, IEpisodeFileRepository
    {
        public EpisodeFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<EpisodeFile> FindByTVShowId(int tvShowId)
        {
            return Query(f => f.TVShowId == tvShowId);
        }

        public List<EpisodeFile> FindBySeasonId(int seasonId)
        {
            return Query(f => f.SeasonId == seasonId);
        }

        public EpisodeFile FindByEpisodeId(int episodeId)
        {
            return Query(f => f.EpisodeId == episodeId).FirstOrDefault();
        }
    }
}
