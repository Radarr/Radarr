using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(241)]
public class add_retroapply_to_importlists : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ImportLists").AddColumn("RetroApplyTags").AsBoolean().WithDefaultValue(false);
        Alter.Table("Notifications").AddColumn("RetroApplyTags").AsBoolean().WithDefaultValue(false);
        Alter.Table("Indexers").AddColumn("RetroApplyTags").AsBoolean().WithDefaultValue(false);
        Alter.Table("DownloadClients").AddColumn("RetroApplyTags").AsBoolean().WithDefaultValue(false);
    }
}