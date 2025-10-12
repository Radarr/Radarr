using System;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ReleaseBranchCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileService;

        public ReleaseBranchCheck(IConfigFileProvider configFileService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configFileService = configFileService;
        }

        public override HealthCheck Check()
        {
            var currentBranch = _configFileService.Branch.ToLowerInvariant();

            if (Enum.GetNames(typeof(ReleaseBranches)).All(x => x.ToLowerInvariant() != currentBranch))
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.ReleaseBranch,
                    string.Format(_localizationService.GetLocalizedString("ReleaseBranchCheckOfficialBranchMessage"), _configFileService.Branch),
                    "#branch-is-not-a-valid-release-branch");
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
