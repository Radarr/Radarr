using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(202)]
    public class remove_predb : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Set PreDb entries to Released
            Update.Table("Movies").Set(new { MinimumAvailability = 3 }).Where(new { MinimumAvailability = 4 });
            Update.Table("ImportLists").Set(new { MinimumAvailability = 3 }).Where(new { MinimumAvailability = 4 });

            //Should never be set, but just in case
            Update.Table("Movies").Set(new { Status = 3 }).Where(new { Status = 4 });
            Update.Table("ImportListMovies").Set(new { Status = 3 }).Where(new { Status = 4 });

            //Remove unused column
            Delete.Column("HasPreDBEntry").FromTable("Movies");
        }
    }
}
