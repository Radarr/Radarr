using System;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ReleaseBranchCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileService;

        public ReleaseBranchCheck(IConfigFileProvider configFileService)
        {
            _configFileService = configFileService;
        }

        public override HealthCheck Check()
        {
            if (!Enum.GetNames(typeof(ReleaseBranches)).Any(x => x.ToLower() == _configFileService.Branch.ToLower()))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Branch {0} is not a valid Radarr release branch, you will not recieve updates", _configFileService.Branch));
            }

            return new HealthCheck(GetType());
        }

        public enum ReleaseBranches
        {
            Develop,
            Nightly,
            Aphrodite
        }
    }
}
