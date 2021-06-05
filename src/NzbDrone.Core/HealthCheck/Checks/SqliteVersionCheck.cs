using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class SqliteVersionCheck : HealthCheckBase
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public SqliteVersionCheck(IMainDatabase database, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _database = database;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (!OsInfo.IsLinux)
            {
                return new HealthCheck(GetType());
            }

            var sqliteVersion = _database.Version;
            var supportedVersion = new Version("3.9.0");

            if (sqliteVersion >= supportedVersion)
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                string.Format(_localizationService.GetLocalizedString("SqliteVersionCheckUpgradeRequiredMessage"), sqliteVersion, supportedVersion),
                "#currently-installed-sqlite-version-is-not-supported");
        }

        public override bool CheckOnSchedule => false;
    }
}
