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
    public class add_minimumavailability : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("Minimumavailability").AsString().Nullable();

        }

    }
}
