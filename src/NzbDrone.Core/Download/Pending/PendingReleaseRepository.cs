using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteByMovieIds(List<int> movieIds);
        List<PendingRelease> AllByMovieId(int movieId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteByMovieIds(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        public List<PendingRelease> AllByMovieId(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<PendingRelease> WithoutFallback()
        {
            return Query(x => x.Reason != PendingReleaseReason.Fallback);
        }
    }
}
