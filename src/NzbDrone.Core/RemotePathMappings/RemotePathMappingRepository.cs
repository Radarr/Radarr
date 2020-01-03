using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.RemotePathMappings
{
    public interface IRemotePathMappingRepository : IBasicRepository<RemotePathMapping>
    {
    }

    public class RemotePathMappingRepository : BasicRepository<RemotePathMapping>, IRemotePathMappingRepository
    {
        public RemotePathMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public new void Delete(int id)
        {
            var model = Get(id);
            base.Delete(id);
            ModelDeleted(model);
        }

        protected override bool PublishModelEvents => true;
    }
}
