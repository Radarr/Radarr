using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(146)]
    public class naming_config_colon_action : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("ColonReplacementFormat").AsString().WithDefaultValue("");
            Update.Table("NamingConfig").Set(new { ColonReplacementFormat = "" }).AllRows();
        }
    }
}
