using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(236)]
    public class clear_last_rss_releases_info : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"IndexerStatus\" SET \"LastRssSyncReleaseInfo\" = NULL");
        }
    }
}
