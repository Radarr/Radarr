using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(107)]
    public class fix_movie_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MovieFiles").AlterColumn("Path").AsString().Nullable(); // Should be deleted, but to much work, ¯\_(ツ)_/¯
        }
    }
}
