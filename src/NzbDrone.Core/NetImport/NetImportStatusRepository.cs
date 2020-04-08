using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.NetImport
{
    public interface INetImportStatusRepository : IProviderStatusRepository<NetImportStatus>
    {
    }

    public class NetImportStatusRepository : ProviderStatusRepository<NetImportStatus>, INetImportStatusRepository
    {
        public NetImportStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
