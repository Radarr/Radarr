using System.Linq;
using System.Runtime.InteropServices;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoNotNetCoreCheck : HealthCheckBase
    {
        private static string[] MonoUnames = new string[] { "OpenBSD", "MidnightBSD", "NetBSD" };
        private readonly IProcessProvider _processProvider;

        public MonoNotNetCoreCheck(IProcessProvider processProvider,
                                   ILocalizationService localizationService)
            : base(localizationService)
        {
            _processProvider = processProvider;
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            // Don't warn on linux x86 - we don't build x86 net core
            if (OsInfo.IsLinux && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("Monox86SupportCheckMessage"), "mono_support_end_of_life");
            }

            // Check for BSD
            var output = _processProvider.StartAndCapture("uname");
            if (output?.ExitCode == 0 && MonoUnames.Contains(output?.Lines.First().Content))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("MonoBSDSupportCheckMessage"), OsInfo.Os), "mono_support_end_of_life");
            }

            return new HealthCheck(GetType(),
                                   HealthCheckResult.Error,
                                   _localizationService.GetLocalizedString("MonoNotNetCoreCheckMessage"),
                                   "#update_to_net_core_version");
        }

        public override bool CheckOnSchedule => false;
    }
}
