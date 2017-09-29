using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(119)]
    public class artist_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Artists")
                  .AddColumn("ArtistType").AsString().Nullable()
                  .AddColumn("Disambiguation").AsString().Nullable();
        }

    }
}
