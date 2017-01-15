using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;


namespace NzbDrone.Core.NetImport
{
    public interface INetImportRepository : IProviderRepository<NetImportDefinition>
    {

    }

    public class NetImportRepository : ProviderRepository<NetImportDefinition>, INetImportRepository
    {
        public NetImportRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}