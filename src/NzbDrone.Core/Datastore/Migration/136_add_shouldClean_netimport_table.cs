using FluentMigrator;
using FluentMigrator.Expressions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(136)]
    public class add_shouldClean_netimport_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!this.Schema.Schema("dbo").Table("NetImport").Column("ShouldClean").Exists())
            {
                Alter.Table("NetImport")
                    .AddColumn("ShouldClean").AsInt32().WithDefaultValue(0);
            }
        }
    }
}
