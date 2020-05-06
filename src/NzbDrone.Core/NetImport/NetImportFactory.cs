using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public interface INetImportFactory : IProviderFactory<INetImport, NetImportDefinition>
    {
        List<INetImport> Enabled();

        List<INetImport> Discoverable();
    }

    public class NetImportFactory : ProviderFactory<INetImport, NetImportDefinition>, INetImportFactory
    {
        private readonly INetImportRepository _providerRepository;
        private readonly Logger _logger;

        public NetImportFactory(INetImportRepository providerRepository,
                              IEnumerable<INetImport> providers,
                              IContainer container,
                              IEventAggregator eventAggregator,
                              Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        protected override List<NetImportDefinition> Active()
        {
            return base.Active().Where(c => c.Enabled).ToList();
        }

        public override void SetProviderCharacteristics(INetImport provider, NetImportDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.ListType = provider.ListType;
        }

        public List<INetImport> Enabled()
        {
            var enabledImporters = GetAvailableProviders().Where(n => ((NetImportDefinition)n.Definition).Enabled);
            var indexers = FilterBlockedIndexers(enabledImporters);
            return indexers.ToList();
        }

        public List<INetImport> Discoverable()
        {
            var enabledImporters = GetAvailableProviders().Where(n => (n.GetType() == typeof(RadarrList.RadarrListImport) || n.GetType() == typeof(TMDb.Popular.TMDbPopularImport)));
            var indexers = FilterBlockedIndexers(enabledImporters);
            return indexers.ToList();
        }

        private IEnumerable<INetImport> FilterBlockedIndexers(IEnumerable<INetImport> importers)
        {
            foreach (var importer in importers)
            {
                yield return importer;
            }
        }
    }
}
