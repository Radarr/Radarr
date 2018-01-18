using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(9)]
    public class album_releases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Albums").AddColumn("Releases").AsString().WithDefaultValue("").Nullable();
            Alter.Table("Albums").AddColumn("CurrentRelease").AsString().WithDefaultValue("").Nullable();
        }
    }
}
