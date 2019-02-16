using System;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class FpcalcCheck : HealthCheckBase
    {
        private readonly IFingerprintingService _fingerprintingService;
        private readonly IConfigService _configService;
        
        public FpcalcCheck(IFingerprintingService fingerprintingService,
                           IConfigService configService)
        {
            _fingerprintingService = fingerprintingService;
            _configService = configService;
        }
        
        public override HealthCheck Check()
        {
            // always pass if fingerprinting is disabled
            if (_configService.AllowFingerprinting == AllowFingerprinting.Never)
            {
                return new HealthCheck(GetType());
            }
            
            if (!_fingerprintingService.IsSetup())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, $"fpcalc could not be found.  Audio fingerprinting disabled.", "#fpcalc-missing");
            }

            var fpcalcVersion = _fingerprintingService.FpcalcVersion();
            if (fpcalcVersion == null || fpcalcVersion < new Version("1.4.3"))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, $"You have an old version of fpcalc.  Please upgrade to 1.4.3.", "#fpcalc-upgrade");
            }

            return new HealthCheck(GetType());
        }
    }
}
