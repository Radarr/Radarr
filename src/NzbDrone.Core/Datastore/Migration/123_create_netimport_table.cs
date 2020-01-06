using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(123)]
    public class create_netimport_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Schema("dbo").Table("NetImport").Exists())
            {
                Create.TableForModel("NetImport")
                    .WithColumn("Enabled").AsBoolean()
                    .WithColumn("Name").AsString().Unique()
                    .WithColumn("Implementation").AsString()
                    .WithColumn("ConfigContract").AsString().Nullable()
                    .WithColumn("Settings").AsString().Nullable()
                    .WithColumn("EnableAuto").AsInt32()
                    .WithColumn("RootFolderPath").AsString()
                    .WithColumn("ShouldMonitor").AsInt32()
                    .WithColumn("ProfileId").AsInt32();
            }
        }
    }
}
