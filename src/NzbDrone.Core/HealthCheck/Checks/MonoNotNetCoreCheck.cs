using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoNotNetCoreCheck : HealthCheckBase
    {
        private static string[] MonoUnames = new string[] { "FreeBSD", "OpenBSD", "MidnightBSD", "NetBSD" };
        private readonly IOsInfo _osInfo;
        private readonly IProcessProvider _processProvider;

        public MonoNotNetCoreCheck(IOsInfo osInfo,
                                   IProcessProvider processProvider,
                                   ILocalizationService localizationService,
                                   Logger logger)
            : base(localizationService)
        {
            _osInfo = osInfo;
            _processProvider = processProvider;
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            // Don't warn on arm based synology - could be arm5 or something else rubbish
            if (_osInfo.Name == "DSM" && RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                return new HealthCheck(GetType());
            }

            // Check for BSD
            var output = _processProvider.StartAndCapture("uname");
            if (output?.ExitCode == 0 && MonoUnames.Contains(output?.Lines.First().Content))
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(),
                                   HealthCheckResult.Warning,
                                   _localizationService.GetLocalizedString("MonoNotNetCoreCheckMessage"),
                                   "#update-to-net-core-version");
        }

        public override bool CheckOnSchedule => false;
    }
}
