using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteByArtistId(int artistId);
        List<PendingRelease> AllByArtistId(int artistId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteByArtistId(int artistId)
        {
            Delete(r => r.ArtistId == artistId);
        }

        public List<PendingRelease> AllByArtistId(int artistId)
        {
            return Query.Where(p => p.ArtistId == artistId);
        }

        public List<PendingRelease> WithoutFallback()
        {
            return Query.Where(p => p.Reason != PendingReleaseReason.Fallback);
        }
    }
}
