using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;


namespace NzbDrone.Core.NetImport
{
    public interface INetImportRepository : IProviderRepository<NetImportDefinition>
    {
        void UpdateSettings(NetImportDefinition model);
    }

    public class NetImportRepository : ProviderRepository<NetImportDefinition>, INetImportRepository
    {
        public NetImportRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void UpdateSettings(NetImportDefinition model)
        {
            SetFields(model, m => m.Settings);
        }
    }
}
