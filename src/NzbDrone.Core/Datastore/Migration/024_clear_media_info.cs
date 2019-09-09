using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(024)]
    public class NewMediaInfoFormat : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("TrackFiles").Set(new { MediaInfo = "" }).AllRows();
        }
    }
}
