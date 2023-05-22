using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(224)]
    public class list_sync_time : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("LastSyncListInfo").FromTable("ImportListStatus");

            Alter.Table("ImportListStatus").AddColumn("LastInfoSync").AsDateTimeOffset().Nullable();

            Delete.FromTable("Config").Row(new { Key = "importlistsyncinterval" });
        }
    }
}
