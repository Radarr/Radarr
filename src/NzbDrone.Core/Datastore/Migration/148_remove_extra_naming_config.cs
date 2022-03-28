using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(148)]
    public class remove_extra_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Remove all but 1 NamingConfig
            Execute.Sql("DELETE FROM \"NamingConfig\" WHERE \"Id\" NOT IN(SELECT \"Id\" FROM \"NamingConfig\" LIMIT 1)");
        }
    }
}
