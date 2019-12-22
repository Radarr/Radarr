using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(122)]
    public class add_movieid_to_blacklist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist").AddColumn("MovieId").AsInt32().Nullable().WithDefaultValue(0);
            Alter.Table("Blacklist").AlterColumn("SeriesId").AsInt32().Nullable();
            Alter.Table("Blacklist").AlterColumn("EpisodeIds").AsString().Nullable();
        }

    }
}
