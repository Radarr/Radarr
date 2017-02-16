using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(133)]
    public class add_allflicksurl : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
	    if (!this.Schema.Schema("dbo").Table("Movies").Column("AllFlicksUrl").Exists())
	    {
                Alter.Table("Movies").AddColumn("AllFlicksUrl").AsString().Nullable();
            }
	}

    }
}
