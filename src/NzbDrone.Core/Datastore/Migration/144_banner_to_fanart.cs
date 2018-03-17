using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(144)]
    public class banner_to_fanart : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Movies SET Images = replace(Images, \'\"coverType\": \"banner\"\', \'\"coverType\": \"fanart\"\')");
        }
    }
}
