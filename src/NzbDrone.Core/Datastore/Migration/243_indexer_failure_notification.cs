using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(243)]
    public class indexer_failure_notification : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnIndexerFailure").AsBoolean().WithDefaultValue(false);

            // Indexer failures were previously sent as health issues, keep sending them for existing notifications
            Execute.Sql("UPDATE \"Notifications\" SET \"OnIndexerFailure\" = \"OnHealthIssue\"");
        }
    }
}
