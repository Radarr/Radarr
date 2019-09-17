using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(37)]
    public class remove_growl_prowl : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "Growl" });
            Delete.FromTable("Notifications").Row(new { Implementation = "Prowl" });
        }
    }
}
