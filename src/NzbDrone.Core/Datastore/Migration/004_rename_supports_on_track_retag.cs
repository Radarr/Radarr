using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(004)]
    public class rename_supports_on_track_retag : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("OnTrackRetag").OnTable("Notifications").To("OnBookRetag");
        }
    }
}
