using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10)]
    public class album_releases_fix : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Releases").FromTable("Albums");
            Delete.Column("CurrentRelease").FromTable("Albums");
            Alter.Table("Albums").AddColumn("Releases").AsString().WithDefaultValue("[]").NotNullable();
            Alter.Table("Albums").AddColumn("CurrentRelease").AsString().WithDefaultValue("").NotNullable();
        }
    }
}
