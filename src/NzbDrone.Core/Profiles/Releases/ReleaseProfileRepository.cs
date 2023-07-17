using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IReleaseProfileRepository : IBasicRepository<ReleaseProfile>
    {
    }

    public class ReleaseProfileRepository : BasicRepository<ReleaseProfile>, IReleaseProfileRepository
    {
        public ReleaseProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
