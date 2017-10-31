using System.Collections.Generic;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Lidarr.Http;

namespace Lidarr.Api.V1.Health
{
    public class HealthModule : LidarrRestModuleWithSignalR<HealthResource, HealthCheck>,
                                IHandle<HealthCheckCompleteEvent>
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthModule(IBroadcastSignalRMessage signalRBroadcaster, IHealthCheckService healthCheckService)
            : base(signalRBroadcaster)
        {
            _healthCheckService = healthCheckService;
            GetResourceAll = GetHealth;
        }

        private List<HealthResource> GetHealth()
        {
            return _healthCheckService.Results().ToResource();
        }

        public void Handle(HealthCheckCompleteEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}