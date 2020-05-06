using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(157)]
    public class remove_growl_prowl : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "Growl" });

            // Prowl Added back
            // Delete.FromTable("Notifications").Row(new { Implementation = "Prowl" });
        }
    }
}
