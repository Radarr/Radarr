using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DatabaseIntegrityCheck : HealthCheckBase
    {
        private readonly IDatabase _mainDatabase;
        private readonly IDatabase _logDatabase;
        private readonly Logger _logger;

        public DatabaseIntegrityCheck(IMainDatabase mainDatabase, ILogDatabase logDatabase, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _mainDatabase = mainDatabase;
            _logDatabase = logDatabase;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var mainDbValid = _mainDatabase.IsValid;
            var logDbValid = _logDatabase.IsValid;

            if (!mainDbValid && !logDbValid)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("DatabaseIntegrityCheckBothFailedMessage"),
                    "#database-failed-integrity-check");
            }

            if (!mainDbValid)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("DatabaseIntegrityCheckMainFailedMessage"),
                    "#database-failed-integrity-check");
            }

            if (!logDbValid)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("DatabaseIntegrityCheckLogFailedMessage"),
                    "#database-failed-integrity-check");
            }

            return new HealthCheck(GetType());
        }
    }
}
