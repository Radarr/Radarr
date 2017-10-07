using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(120)]
    public class artist_album_types : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Artists")
                  .AddColumn("PrimaryAlbumTypes").AsString().Nullable()
                  .AddColumn("SecondaryAlbumTypes").AsString().Nullable();
        }

    }
}
