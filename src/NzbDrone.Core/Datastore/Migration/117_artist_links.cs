using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(117)]
    public class artist_links : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Artists")
                  .AddColumn("Links").AsString().Nullable();
        }

    }
}
