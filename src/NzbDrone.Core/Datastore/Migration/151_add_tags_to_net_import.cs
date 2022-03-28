using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(151)]
    public class add_tags_to_net_import : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NetImport")
                 .AddColumn("Tags").AsString().Nullable();

            Execute.Sql("UPDATE \"NetImport\" SET \"Tags\" = '[]'");
        }
    }
}
