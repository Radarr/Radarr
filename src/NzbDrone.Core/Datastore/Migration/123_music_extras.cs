using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(123)]
    public class music_extras : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ExtraFiles")
                  .AddColumn("ArtistId").AsInt32().NotNullable().WithDefaultValue(0)
                  .AddColumn("AlbumId").AsInt32().NotNullable().WithDefaultValue(0)
                  .AddColumn("TrackFileId").AsInt32().NotNullable().WithDefaultValue(0);

            Delete.Column("SeriesId").FromTable("ExtraFiles");
            Delete.Column("SeasonNumber").FromTable("ExtraFiles");
            Delete.Column("EpisodeFileId").FromTable("ExtraFiles");

            Alter.Table("SubtitleFiles")
                  .AddColumn("ArtistId").AsInt32().NotNullable().WithDefaultValue(0)
                  .AddColumn("AlbumId").AsInt32().NotNullable().WithDefaultValue(0)
                  .AddColumn("TrackFileId").AsInt32().NotNullable().WithDefaultValue(0);

            Delete.Column("SeriesId").FromTable("SubtitleFiles");
            Delete.Column("SeasonNumber").FromTable("SubtitleFiles");
            Delete.Column("EpisodeFileId").FromTable("SubtitleFiles");

            Alter.Table("MetadataFiles")
                  .AddColumn("ArtistId").AsInt32().NotNullable().WithDefaultValue(0)
                  .AddColumn("AlbumId").AsInt32().Nullable()
                  .AddColumn("TrackFileId").AsInt32().Nullable();

            Delete.Column("SeriesId").FromTable("MetadataFiles");
            Delete.Column("SeasonNumber").FromTable("MetadataFiles");
            Delete.Column("EpisodeFileId").FromTable("MetadataFiles");
        }

    }
}
