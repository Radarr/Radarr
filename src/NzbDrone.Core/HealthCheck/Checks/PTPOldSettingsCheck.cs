using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.PassThePopcorn;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class PTPOldSettingsCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;
        
        public PTPOldSettingsCheck(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }
        
        public override HealthCheck Check()
        {
            var ptpIndexers = _indexerFactory.All().Where(i => i.Settings.GetType() == typeof(PassThePopcornSettings));

            var ptpIndexerOldSettings = ptpIndexers
                .Where(i => (i.Settings as PassThePopcornSettings).APIUser.IsNullOrWhiteSpace()).Select(i => i.Name);

            if (ptpIndexerOldSettings.Count() > 0)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, $"The following PassThePopcorn indexers have deprecated settings and should be updated: {string.Join(",", ptpIndexerOldSettings)}");
            }
            
            return new HealthCheck(GetType());
        }
    }
}
