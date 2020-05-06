using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoVersionCheck : HealthCheckBase
    {
        private readonly IPlatformInfo _platformInfo;
        private readonly Logger _logger;

        public MonoVersionCheck(IPlatformInfo platformInfo, Logger logger)
        {
            _platformInfo = platformInfo;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            var monoVersion = _platformInfo.Version;

            // Currently best stable Mono version (5.18 gets us .net 4.7.2 support)
            var bestVersion = new Version("5.20");
            if (monoVersion >= bestVersion)
            {
                _logger.Debug("Mono version is {0} or better: {1}", bestVersion, monoVersion);
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                $"Currently installed Mono version {monoVersion} is old and unsupported. Please upgrade Mono to version {bestVersion}.",
                "#currently-installed-mono-version-is-old-and-unsupported");
        }

        public override bool CheckOnSchedule => false;
    }
}
