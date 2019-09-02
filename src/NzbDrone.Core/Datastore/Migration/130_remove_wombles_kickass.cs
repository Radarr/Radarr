using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(130)]
    public class remove_wombles_kickass : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "Wombles" });
            Delete.FromTable("Indexers").Row(new { Implementation = "KickassTorrents" });
        }
    }
}