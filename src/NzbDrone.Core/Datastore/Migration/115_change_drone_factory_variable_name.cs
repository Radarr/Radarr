using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(115)]
    public class change_drone_factory_variable_name : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM Config WHERE [Key] = 'downloadedepisodesfolder'");
            Execute.Sql("DELETE FROM Config WHERE [Key] = 'downloadedepisodesscaninterval'");
        }

    }
}
