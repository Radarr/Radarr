using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(20)]
    public class remove_pushalot : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "Pushalot" });
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
            Delete.FromTable("MetadataFiles").Row(new { Consumer = "MediaBrowserMetadata" });
        }
    }
}
