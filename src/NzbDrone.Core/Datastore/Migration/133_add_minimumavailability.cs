using FluentMigrator;
//using FluentMigrator.Expressions;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(133)]
    public class add_minimumavailability : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!this.Schema.Schema("dbo").Table("NetImport").Column("Minimumavailability").Exists())
            {
                Alter.Table("NetImport").AddColumn("Minimumavailability").AsInt32().WithDefaultValue(MovieStatusType.Released);
            }
	        if (!this.Schema.Schema("dbo").Table("Movies").Column("Minimumavailability").Exists())
	        {
		        Alter.Table("Movies").AddColumn("Minimumavailability").AsInt32().WithDefaultValue(MovieStatusType.Released);
	        }
        }
    }
}
