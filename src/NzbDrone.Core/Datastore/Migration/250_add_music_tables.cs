using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(250)]
    public class add_music_tables : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Artists")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("SortName").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignArtistId").AsString().Nullable()
                .WithColumn("DiscogsId").AsString().Nullable()
                .WithColumn("ArtistType").AsString().Nullable()
                .WithColumn("Status").AsString().Nullable()
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]");

            Create.TableForModel("Albums")
                .WithColumn("ArtistId").AsInt32().Nullable()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignAlbumId").AsString().Nullable()
                .WithColumn("DiscogsId").AsString().Nullable()
                .WithColumn("MediaType").AsInt32().WithDefaultValue(3)
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("AlbumType").AsString().Nullable()
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]")
                .WithColumn("LastSearchTime").AsDateTime().Nullable();

            Create.TableForModel("Tracks")
                .WithColumn("AlbumId").AsInt32().Nullable()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("ForeignTrackId").AsString().Nullable()
                .WithColumn("TrackNumber").AsInt32().WithDefaultValue(0)
                .WithColumn("DiscNumber").AsInt32().WithDefaultValue(1)
                .WithColumn("DurationSeconds").AsInt32().Nullable()
                .WithColumn("MediaType").AsInt32().WithDefaultValue(3)
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]")
                .WithColumn("LastSearchTime").AsDateTime().Nullable()
                .WithColumn("AuthorId").AsInt32().Nullable()
                .WithColumn("SeriesId").AsInt32().Nullable();

            Create.TableForModel("MusicFiles")
                .WithColumn("TrackId").AsInt32().Nullable()
                .WithColumn("AlbumId").AsInt32().Nullable()
                .WithColumn("RelativePath").AsString().Nullable()
                .WithColumn("Size").AsInt64().WithDefaultValue(0)
                .WithColumn("DateAdded").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("Quality").AsString().WithDefaultValue("{}")
                .WithColumn("AudioFormat").AsString().Nullable()
                .WithColumn("Bitrate").AsInt32().Nullable()
                .WithColumn("SampleRate").AsInt32().Nullable()
                .WithColumn("Channels").AsInt32().Nullable();

            Create.Index("IX_Albums_ArtistId").OnTable("Albums").OnColumn("ArtistId");
            Create.Index("IX_Albums_ArtistId_Monitored").OnTable("Albums")
                .OnColumn("ArtistId").Ascending()
                .OnColumn("Monitored").Ascending();
            Create.Index("IX_Tracks_AlbumId").OnTable("Tracks").OnColumn("AlbumId");
            Create.Index("IX_MusicFiles_TrackId").OnTable("MusicFiles").OnColumn("TrackId");
            Create.Index("IX_MusicFiles_AlbumId").OnTable("MusicFiles").OnColumn("AlbumId");
        }
    }
}
