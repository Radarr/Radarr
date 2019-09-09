using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(027)]
    public class add_import_exclusions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("ImportListExclusions")
                  .WithColumn("ForeignId").AsString().NotNullable().Unique()
                  .WithColumn("Name").AsString().NotNullable();
        }
    }
}
