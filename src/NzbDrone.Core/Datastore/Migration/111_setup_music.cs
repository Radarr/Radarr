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
                .WithColumn("ItunesId").AsInt32().Unique()
                .WithColumn("ArtistName").AsString().Unique()
                .WithColumn("ArtistSlug").AsString().Unique()
                .WithColumn("CleanTitle").AsString() // Do we need this?
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("AlbumFolder").AsBoolean()
                .WithColumn("ArtistFolder").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Path").AsString()
                .WithColumn("Images").AsString()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("Added").AsDateTime()
                .WithColumn("ProfileId").AsInt32() // This is either ProfileId or Profile
                .WithColumn("Genres").AsString()
                .WithColumn("Albums").AsString()
                .WithColumn("Tags").AsString()
                .WithColumn("AddOptions").AsString()

                ;

            Create.TableForModel("Albums")
                .WithColumn("AlbumId").AsInt32()
                .WithColumn("ArtistId").AsInt32()
                .WithColumn("Title").AsString()
                .WithColumn("Year").AsInt32()
                .WithColumn("Image").AsInt32()
                .WithColumn("TrackCount").AsInt32()
                .WithColumn("DiscCount").AsInt32()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("Overview").AsString();

            Create.TableForModel("Tracks")
                .WithColumn("ItunesTrackId").AsInt32().Unique()
                .WithColumn("AlbumId").AsInt32()
                .WithColumn("ArtistsId").AsString().Nullable()
                .WithColumn("TrackNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Ignored").AsBoolean().Nullable()
                .WithColumn("Explict").AsBoolean()
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
