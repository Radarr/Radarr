using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;


namespace NzbDrone.Core.AutoImport
{
    public interface IAutoImportRepository : IProviderRepository<AutoImportDefinition>
    {

    }

    public class AutoImportRepository : ProviderRepository<AutoImportDefinition>, IAutoImportRepository
    {
        public AutoImportRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}