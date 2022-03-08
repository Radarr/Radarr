using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(190)]
    public class update_awesome_hd_link : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'https://awesome-hd.net', 'https://awesome-hd.club') WHERE \"Implementation\" = 'AwesomeHD';");
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'https://awesome-hd.me', 'https://awesome-hd.club') WHERE \"Implementation\" = 'AwesomeHD';");
        }
    }
}
