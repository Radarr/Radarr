using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(173)]
    public class net_import_status : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("NetImportStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastSyncListInfo").AsString().Nullable();
        }
    }
}
