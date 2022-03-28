using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(192)]
    public class add_on_delete_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("OnDelete").OnTable("Notifications").To("OnMovieDelete");
            Alter.Table("Notifications").AddColumn("OnMovieFileDelete").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("OnMovieFileDeleteForUpgrade").AsBoolean().WithDefaultValue(false);
        }
    }
}
