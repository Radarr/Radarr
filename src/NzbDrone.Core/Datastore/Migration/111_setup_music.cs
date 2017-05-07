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
            Create.TableForModel("Artist")
                .WithColumn("SpotifyId").AsString().Nullable().Unique()
                .WithColumn("ArtistName").AsString().Unique()
                .WithColumn("ArtistSlug").AsString().Nullable() //.Unique()
                .WithColumn("CleanTitle").AsString().Nullable() // Do we need this?
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("AlbumFolder").AsBoolean().Nullable()
                .WithColumn("ArtistFolder").AsBoolean().Nullable()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Status").AsInt32().Nullable()
                .WithColumn("Path").AsString()
                .WithColumn("Images").AsString().Nullable()
                .WithColumn("QualityProfileId").AsInt32().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("ProfileId").AsInt32().Nullable() // This is either ProfileId or Profile
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Albums").AsString().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("AddOptions").AsString().Nullable()
                ;

            Create.TableForModel("Albums")
                .WithColumn("AlbumId").AsString().Unique()
                .WithColumn("ArtistId").AsInt32() // Should this be artistId (string)
                .WithColumn("Title").AsString()
                .WithColumn("Year").AsInt32()
                .WithColumn("Image").AsInt32()
                .WithColumn("TrackCount").AsInt32()
                .WithColumn("DiscCount").AsInt32()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("Overview").AsString();

            Create.TableForModel("Tracks")
                .WithColumn("ItunesTrackId").AsInt32().Unique()
                .WithColumn("AlbumId").AsString()
                .WithColumn("ArtistsId").AsString().Nullable()
                .WithColumn("TrackNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Ignored").AsBoolean().Nullable()
                .WithColumn("Explict").AsBoolean()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("TrackExplicitName").AsString().Nullable()
                .WithColumn("TrackCensoredName").AsString().Nullable()
                .WithColumn("TrackFileId").AsInt32().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable();


            Create.TableForModel("TrackFiles")
                  .WithColumn("ArtistId").AsInt32()
                  .WithColumn("Path").AsString().Unique()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime()
                  .WithColumn("AlbumId").AsInt32(); // How does this impact stand alone tracks?

        }

    }
}
