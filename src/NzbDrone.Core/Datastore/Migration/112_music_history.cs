using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(112)]
    public class music_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                  .AddColumn("ArtistId").AsInt32().WithDefaultValue(0)
                  .AddColumn("AlbumId").AsInt32().WithDefaultValue(0);

            Alter.Table("PendingReleases")
                  .AddColumn("ArtistId").AsInt32().WithDefaultValue(0)
                  .AddColumn("ParsedAlbumInfo").AsString().WithDefaultValue("");

            Alter.Table("Tracks")
                  .AddColumn("Duration").AsInt32().WithDefaultValue(0);

            Alter.Table("Albums")
                  .AddColumn("Duration").AsInt32().WithDefaultValue(0);

            Delete.Column("SeriesId").FromTable("History");
            Delete.Column("EpisodeId").FromTable("History");
            Delete.Column("SeriesId").FromTable("PendingReleases");
            Delete.Column("ParsedEpisodeInfo").FromTable("PendingReleases");
        }

    }
}
