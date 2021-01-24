using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoNotNetCoreCheck : HealthCheckBase
    {
        private static string[] MonoUnames = new string[] { "FreeBSD", "OpenBSD", "MidnightBSD", "NetBSD" };
        private readonly IOsInfo _osInfo;
        private readonly IProcessProvider _processProvider;

        public MonoNotNetCoreCheck(IOsInfo osInfo,
                                   IProcessProvider processProvider,
                                   Logger logger)
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

            // Don't warn on linux x86 - we don't build x86 net core
            if (OsInfo.IsLinux && RuntimeInformation.ProcessArchitecture == Architecture.X86)
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
                                   "Please upgrade to the .NET Core version of Readarr",
                                   "#update-to-net-core-version");
        }

        public override bool CheckOnSchedule => false;
    }
}
