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
    }

    public class NetImportFactory : ProviderFactory<INetImport, NetImportDefinition>, INetImportFactory
    {
        //private readonly IIndexerStatusService _indexerStatusService;
        private readonly INetImportRepository _providerRepository;
        private readonly Logger _logger;

        public NetImportFactory(//IIndexerStatusService indexerStatusService,
                              INetImportRepository providerRepository,
                              IEnumerable<INetImport> providers,
                              IContainer container, 
                              IEventAggregator eventAggregator,
                              Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            //_indexerStatusService = indexerStatusService;
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
        }

        public List<INetImport> Enabled()
        {
            var enabledIndexers = GetAvailableProviders().Where(n => ((NetImportDefinition)n.Definition).Enabled);

            var indexers = FilterBlockedIndexers(enabledIndexers);

            return indexers.ToList();
        }

        private IEnumerable<INetImport> FilterBlockedIndexers(IEnumerable<INetImport> indexers)
        {
            //var blockedIndexers = _indexerStatusService.GetBlockedIndexers().ToDictionary(v => v.IndexerId, v => v);

            foreach (var indexer in indexers)
            {
                /*IndexerStatus blockedIndexerStatus;
                if (blockedIndexers.TryGetValue(indexer.Definition.Id, out blockedIndexerStatus))
                {
                    _logger.Debug("Temporarily ignoring indexer {0} till {1} due to recent failures.", indexer.Definition.Name, blockedIndexerStatus.DisabledTill.Value.ToLocalTime());
                    continue;
                }*/

                yield return indexer;
            }
        }
    }
}