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
        private readonly INetImportStatusService _netImportStatusService;
        private readonly Logger _logger;

        public NetImportFactory(INetImportRepository providerRepository,
                                INetImportStatusService netImportStatusService,
                                IEnumerable<INetImport> providers,
                                IContainer container,
                                IEventAggregator eventAggregator,
                                Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _netImportStatusService = netImportStatusService;
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
            var indexers = FilterBlockedNetImport(enabledImporters);
            return indexers.ToList();
        }

        public List<INetImport> Discoverable()
        {
            var enabledImporters = GetAvailableProviders().Where(n => (n.GetType() == typeof(RadarrList.RadarrListImport) || n.GetType() == typeof(TMDb.Popular.TMDbPopularImport)));
            var indexers = FilterBlockedNetImport(enabledImporters);
            return indexers.ToList();
        }

        private IEnumerable<INetImport> FilterBlockedNetImport(IEnumerable<INetImport> importers)
        {
            var blockedLists = _netImportStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

            foreach (var importer in importers)
            {
                NetImportStatus netImportStatus;
                if (blockedLists.TryGetValue(importer.Definition.Id, out netImportStatus))
                {
                    _logger.Debug("Temporarily ignoring list {0} till {1} due to recent failures.", importer.Definition.Name, netImportStatus.DisabledTill.Value.ToLocalTime());
                    continue;
                }

                yield return importer;
            }
        }
    }
}
