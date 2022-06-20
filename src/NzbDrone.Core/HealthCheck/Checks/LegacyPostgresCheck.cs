using System;
using System.Collections;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class LegacyPostgresCheck : HealthCheckBase
    {
        private readonly Logger _logger;

        public LegacyPostgresCheck(ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var legacyVars = Environment
                .GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Select(x => x.Key.ToString())
                .Where(k => k.StartsWith(BuildInfo.AppName + "__Postgres__") || k.StartsWith(BuildInfo.AppName + ":Postgres:"))
                .ToList();

            if (legacyVars.Count == 0)
            {
                return new HealthCheck(GetType());
            }

            var legacyString = legacyVars.OrderBy(x => x).ConcatToString();
            var newString = legacyString
                .Replace(BuildInfo.AppName + "__Postgres__", BuildInfo.AppName + "__Postgres")
                .Replace(BuildInfo.AppName + ":Postgres:", BuildInfo.AppName + ":Postgres");

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                string.Format(_localizationService.GetLocalizedString("PostgresLegacyEnvironmentVariables"), legacyString, newString));
        }

        public override bool CheckOnSchedule => false;
    }
}
