using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(136)]
    public class add_pathstate_to_movies : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("PathState").AsInt32().WithDefaultValue(2);
        }
    }
}
