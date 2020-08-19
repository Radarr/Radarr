using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(2)]
    public class ImportListSearch : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("ShouldSearch").AsInt32().WithDefaultValue(1);
        }
    }
}
