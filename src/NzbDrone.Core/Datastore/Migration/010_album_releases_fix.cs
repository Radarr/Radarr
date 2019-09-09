using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10)]
    public class album_releases_fix : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Albums").AlterColumn("Releases").AsString().NotNullable();
            Alter.Table("Albums").AlterColumn("CurrentRelease").AsString().NotNullable();
        }
    }
}
