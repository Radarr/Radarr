using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.PassThePopcorn;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class PTPOldSettingsCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public PTPOldSettingsCheck(IIndexerFactory indexerFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var ptpIndexers = _indexerFactory.All().Where(i => i.Settings.GetType() == typeof(PassThePopcornSettings));

            var ptpIndexerOldSettings = ptpIndexers
                .Where(i => (i.Settings as PassThePopcornSettings).APIUser.IsNullOrWhiteSpace()).Select(i => i.Name);

            if (ptpIndexerOldSettings.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format(_localizationService.GetLocalizedString("PtpOldSettingsCheckMessage"), string.Join(", ", ptpIndexerOldSettings)), "#ptp_settings_old");
            }

            return new HealthCheck(GetType());
        }
    }
}
