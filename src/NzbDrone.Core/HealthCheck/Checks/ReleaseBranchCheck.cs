using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ReleaseBranchCheck : HealthCheckBase
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileService;

        public ReleaseBranchCheck(IOptionsMonitor<ConfigFileOptions> configFileService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configFileService = configFileService;
        }

        public override HealthCheck Check()
        {
            var currentBranch = _configFileService.CurrentValue.Branch.ToLower();

            if (!Enum.GetNames(typeof(ReleaseBranches)).Any(x => x.ToLower() == currentBranch))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format(_localizationService.GetLocalizedString("ReleaseBranchCheckOfficialBranchMessage"), _configFileService.CurrentValue.Branch), "#branch-is-not-a-valid-release-branch");
            }

            return new HealthCheck(GetType());
        }

        public enum ReleaseBranches
        {
            Master,
            Develop,
            Nightly
        }
    }
}
