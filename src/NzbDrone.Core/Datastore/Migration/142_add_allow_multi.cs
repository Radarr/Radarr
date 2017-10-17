using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(142)]
    public class add_allow_multi : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Profiles").AddColumn("AllowMulti").AsBoolean().Nullable();
        }
    }
}
