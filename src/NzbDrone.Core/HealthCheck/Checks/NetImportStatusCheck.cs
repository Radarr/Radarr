using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<INetImport>))]
    [CheckOn(typeof(ProviderDeletedEvent<INetImport>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<INetImport>))]
    public class NetImportStatusCheck : HealthCheckBase
    {
        private readonly INetImportFactory _providerFactory;
        private readonly INetImportStatusService _providerStatusService;

        public NetImportStatusCheck(INetImportFactory providerFactory, INetImportStatusService providerStatusService)
        {
            _providerFactory = providerFactory;
            _providerStatusService = providerStatusService;
        }

        public override HealthCheck Check()
        {
            var enabledProviders = _providerFactory.GetAvailableProviders();
            var backOffProviders = enabledProviders.Join(_providerStatusService.GetBlockedProviders(),
                    i => i.Definition.Id,
                    s => s.ProviderId,
                    (i, s) => new { Provider = i, Status = s })
                .ToList();

            if (backOffProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffProviders.Count == enabledProviders.Count)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "All lists are unavailable due to failures", "#lists-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Lists unavailable due to failures: {0}", string.Join(", ", backOffProviders.Select(v => v.Provider.Definition.Name))), "#lists-are-unavailable-due-to-failures");
        }
    }
}
