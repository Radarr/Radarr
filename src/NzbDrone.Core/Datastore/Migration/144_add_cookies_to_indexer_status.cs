using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(144)]
    public class add_cookies_to_indexer_status : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("IndexerStatus").AddColumn("Cookies").AsString().Nullable()
                .AddColumn("CookiesExpirationDate").AsDateTime().Nullable();
            
        }
    }
}
