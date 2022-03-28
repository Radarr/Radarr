using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(155)]
    public class add_update_allowed_quality_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Profiles").AddColumn("UpgradeAllowed").AsBoolean().Nullable();

            // Set upgrade allowed for existing profiles (default will be false for new profiles)
            Update.Table("Profiles").Set(new { UpgradeAllowed = true }).AllRows();
        }
    }
}
