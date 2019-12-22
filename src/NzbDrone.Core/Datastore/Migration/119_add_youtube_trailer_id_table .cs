using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(119)]
    public class add_youtube_trailer_id : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("YouTubeTrailerId").AsString().Nullable();
        }
    }
}
