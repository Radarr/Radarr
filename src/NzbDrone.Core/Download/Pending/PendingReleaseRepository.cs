using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteByMovieId(int movieId);
        List<PendingRelease> AllByMovieId(int movieId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteByMovieId(int movieId)
        {
            Delete(r => r.MovieId == movieId);
        }

        public List<PendingRelease> AllByMovieId(int movieId)
        {
            return Query.Where(p => p.MovieId == movieId).ToList();
        }

        public List<PendingRelease> WithoutFallback()
        {
            return Query.Where(p => p.Reason != PendingReleaseReason.Fallback);
        }
    }
}
