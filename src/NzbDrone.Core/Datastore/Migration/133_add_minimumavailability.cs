using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(133)]
    public class add_minimumavailability : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Table("NetImport").Column("MinimumAvailability").Exists())
            {
                Alter.Table("NetImport").AddColumn("MinimumAvailability").AsInt32().WithDefaultValue((int)MovieStatusType.Released);
            }

            if (!Schema.Table("Movies").Column("MinimumAvailability").Exists())
            {
                Alter.Table("Movies").AddColumn("MinimumAvailability").AsInt32().WithDefaultValue((int)MovieStatusType.Released);
            }
        }
    }
}
