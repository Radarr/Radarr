using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(234)]
    public class movie_last_searched_time : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("LastSearchTime").AsDateTimeOffset().Nullable();
        }
    }
}
