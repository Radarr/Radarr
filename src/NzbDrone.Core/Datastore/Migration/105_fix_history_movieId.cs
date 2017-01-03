using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(105)]
    public class fix_history_movieId : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                  .AddColumn("MovieId").AsInt32().WithDefaultValue(0);
        }
    }
}
