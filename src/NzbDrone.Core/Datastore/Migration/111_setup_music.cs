using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(111)]
    public class setup_music : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Artists")
                .WithColumn("ForeignArtistId").AsString().Unique()
                .WithColumn("MBId").AsString().Nullable()
                .WithColumn("AMId").AsString().Nullable()
                .WithColumn("TADBId").AsInt32().Nullable()
                .WithColumn("DiscogsId").AsInt32().Nullable()
                .WithColumn("Name").AsString()
                .WithColumn("NameSlug").AsString().Nullable().Unique()
                .WithColumn("CleanName").AsString().Indexed()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString().Indexed()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("AlbumFolder").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("DateFormed").AsDateTime().Nullable()
                .WithColumn("Members").AsString().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("SortName").AsString().Nullable()
                .WithColumn("ProfileId").AsInt32().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("AddOptions").AsString().Nullable();

            Create.TableForModel("Albums")
                .WithColumn("ForeignAlbumId").AsString().Unique()
                .WithColumn("ArtistId").AsInt32()
                .WithColumn("MBId").AsString().Nullable().Indexed()
                .WithColumn("AMId").AsString().Nullable()
                .WithColumn("TADBId").AsInt32().Nullable().Indexed()
                .WithColumn("DiscogsId").AsInt32().Nullable()
                .WithColumn("Title").AsString()
                .WithColumn("TitleSlug").AsString().Nullable().Unique()
                .WithColumn("CleanTitle").AsString().Indexed()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString().Indexed()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Label").AsString().Nullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("ProfileId").AsInt32().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("AlbumType").AsString()
                .WithColumn("AddOptions").AsString().Nullable();

            Create.TableForModel("Tracks")
                .WithColumn("ForeignTrackId").AsString().Unique()
                .WithColumn("ArtistId").AsInt32().Indexed()
                .WithColumn("AlbumId").AsInt32()
                .WithColumn("MBId").AsString().Nullable().Indexed()
                .WithColumn("TrackNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Explicit").AsBoolean()
                .WithColumn("Compilation").AsBoolean()
                .WithColumn("DiscNumber").AsInt32().Nullable()
                .WithColumn("TrackFileId").AsInt32().Nullable().Indexed()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("Ratings").AsString().Nullable();

            Create.Index().OnTable("Tracks").OnColumn("ArtistId").Ascending()
                                            .OnColumn("AlbumId").Ascending()
                                            .OnColumn("TrackNumber").Ascending();

            Create.TableForModel("TrackFiles")
                  .WithColumn("ArtistId").AsInt32().Indexed()
                  .WithColumn("AlbumId").AsInt32().Indexed()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable()
                  .WithColumn("MediaInfo").AsString().Nullable()
                  .WithColumn("RelativePath").AsString().Nullable();

            Alter.Table("NamingConfig")
                  .AddColumn("ArtistFolderFormat").AsString().Nullable()
                  .AddColumn("RenameTracks").AsBoolean().Nullable()
                  .AddColumn("StandardTrackFormat").AsString().Nullable()
                  .AddColumn("AlbumFolderFormat").AsString().Nullable();
        }

    }
}
