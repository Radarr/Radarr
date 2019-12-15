using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(162)]
    public class fix_profile_format_default : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // badValue was set as default in 147 but it's invalid JSON
            // so System.Text.Json refuses to parse it (even though Newtonsoft allows it)
            var badValue = "[{format:0, allowed:true}]";
            var defaultValue = "[{\"format\":0, \"allowed\":true}]";
            Alter.Column("FormatItems").OnTable("Profiles").AsString().WithDefaultValue(defaultValue);

            Update.Table("Profiles").Set(new { FormatItems = defaultValue }).Where( new { FormatItems = badValue });
        }
    }
}
