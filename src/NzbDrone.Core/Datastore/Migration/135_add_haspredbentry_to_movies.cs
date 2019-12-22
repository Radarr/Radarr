using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(135)]
    public class add_haspredbentry_to_movies : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("HasPreDBEntry").AsBoolean().WithDefaultValue(false);
        }
    }
}
