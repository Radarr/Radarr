using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListFactory : IProviderFactory<IImportList, ImportListDefinition>
    {
        List<IImportList> Enabled();
        List<IImportList> Discoverable();
    }

    public class ImportListFactory : ProviderFactory<IImportList, ImportListDefinition>, IImportListFactory
    {
        private readonly IImportListRepository _providerRepository;
        private readonly Logger _logger;

        public ImportListFactory(IImportListRepository providerRepository,
                              IEnumerable<IImportList> providers,
                              IServiceProvider container,
                              IEventAggregator eventAggregator,
                              Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        protected override List<ImportListDefinition> Active()
        {
            return base.Active().Where(c => c.Enabled).ToList();
        }

        public override void SetProviderCharacteristics(IImportList provider, ImportListDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.ListType = provider.ListType;
        }

        public List<IImportList> Enabled()
        {
            var enabledImporters = GetAvailableProviders().Where(n => ((ImportListDefinition)n.Definition).Enabled);
            return enabledImporters.ToList();
        }

        public List<IImportList> Discoverable()
        {
            var enabledImporters = GetAvailableProviders().Where(n => (n.GetType() == typeof(RadarrList.RadarrListImport) || n.GetType() == typeof(TMDb.Popular.TMDbPopularImport)));
            return enabledImporters.ToList();
        }
    }
}
