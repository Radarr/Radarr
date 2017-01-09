using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(112)]
    public class remove_torrentleech : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "Torrentleech" });
        }
    }
}
