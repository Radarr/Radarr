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
            var currentBranch = _configFileService.Branch.ToLower();

            if (!Enum.GetNames(typeof(ReleaseBranches)).Any(x => x.ToLower() == currentBranch))
            {
                if (currentBranch == "develop" || currentBranch == "nightly")
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Branch {0} is for a previous version of Radarr, set branch to 'Aphrodite' for further updates", _configFileService.Branch));
                }

                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Branch {0} is not a valid Radarr release branch, you will not receive updates", _configFileService.Branch));
            }

            return new HealthCheck(GetType());
        }

        public enum ReleaseBranches
        {
            Aphrodite
        }
    }
}
