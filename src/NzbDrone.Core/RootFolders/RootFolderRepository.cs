using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;

        public new void Delete(int id)
        {
            var model = Get(id);
            base.Delete(id);
            ModelDeleted(model);
        }
    }
}
