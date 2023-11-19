using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(234)]
    public class add_indexer_id_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blocklist").AddColumn("IndexerId").AsInt32().WithDefaultValue(-1);
            Alter.Table("MovieFiles").AddColumn("IndexerId").AsInt32().WithDefaultValue(-1);
        }
    }
}
