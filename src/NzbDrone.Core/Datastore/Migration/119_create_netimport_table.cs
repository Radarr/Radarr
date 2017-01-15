using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(119)]
    public class create_netimport_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("NetImport")
                  .WithColumn("Enabled").AsBoolean()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("Settings").AsString().Nullable();
        }
    }
}
