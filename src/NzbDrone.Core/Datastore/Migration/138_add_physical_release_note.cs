using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(138)]
    public class add_physical_release_note : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
			Alter.Table("Movies").AddColumn("PhysicalReleaseNote").AsString().Nullable();
        }
    }
}
