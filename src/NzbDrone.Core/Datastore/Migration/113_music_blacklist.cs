using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(113)]
    public class music_blacklist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist")
                  .AddColumn("ArtistId").AsInt32().WithDefaultValue(0)
                  .AddColumn("AlbumIds").AsString().WithDefaultValue("");

            Delete.Column("SeriesId").FromTable("Blacklist");
            Delete.Column("EpisodeIds").FromTable("Blacklist");
        }

    }
}
