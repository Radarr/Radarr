using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(1)]
    public class InitialSetup : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Config")
                  .WithColumn("Key").AsString().Unique()
                  .WithColumn("Value").AsString();

            Create.TableForModel("RootFolders")
                  .WithColumn("Path").AsString().Unique();

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
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("LanguageProfileId").AsInt32().WithDefaultValue(1)
                .WithColumn("Links").AsString().Nullable()
                .WithColumn("ArtistType").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("PrimaryAlbumTypes").AsString().Nullable()
                .WithColumn("SecondaryAlbumTypes").AsString().Nullable();

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
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("Duration").AsInt32().WithDefaultValue(0);


            Create.TableForModel("Tracks")
                .WithColumn("ForeignTrackId").AsString().Unique()
                .WithColumn("ArtistId").AsInt32().Indexed()
                .WithColumn("AlbumId").AsInt32()
                .WithColumn("TrackNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Explicit").AsBoolean()
                .WithColumn("Compilation").AsBoolean()
                .WithColumn("DiscNumber").AsInt32().Nullable()
                .WithColumn("TrackFileId").AsInt32().Nullable().Indexed()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Duration").AsInt32().WithDefaultValue(0);

            Create.Index().OnTable("Tracks").OnColumn("ArtistId").Ascending()
                                            .OnColumn("AlbumId").Ascending()
                                            .OnColumn("TrackNumber").Ascending();

            Create.TableForModel("TrackFiles")
                .WithColumn("ArtistId").AsInt32().Indexed()
                .WithColumn("AlbumId").AsInt32().Indexed()
                .WithColumn("Quality").AsString()
                .WithColumn("Size").AsInt64()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("DateAdded").AsDateTime()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("MediaInfo").AsString().Nullable()
                .WithColumn("RelativePath").AsString().Nullable()
                .WithColumn("Language").AsInt32().WithDefaultValue(0);

            Create.TableForModel("History")
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Date").AsDateTime().Indexed()
                .WithColumn("Quality").AsString()
                .WithColumn("Data").AsString()
                .WithColumn("EventType").AsInt32().Nullable().Indexed()
                .WithColumn("DownloadId").AsString().Nullable().Indexed()
                .WithColumn("Language").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("ArtistId").AsInt32().WithDefaultValue(0)
                .WithColumn("AlbumId").AsInt32().Indexed().WithDefaultValue(0)
                .WithColumn("TrackId").AsInt32().WithDefaultValue(0);

            Create.TableForModel("Notifications")
                .WithColumn("Name").AsString()
                .WithColumn("OnGrab").AsBoolean()
                .WithColumn("OnDownload").AsBoolean()
                .WithColumn("Settings").AsString()
                .WithColumn("Implementation").AsString()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("OnUpgrade").AsBoolean().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("OnRename").AsBoolean().NotNullable();

            Create.TableForModel("ScheduledTasks")
                .WithColumn("TypeName").AsString().Unique()
                .WithColumn("Interval").AsInt32()
                .WithColumn("LastExecution").AsDateTime();

            Create.TableForModel("Indexers")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("EnableRss").AsBoolean().Nullable()
                .WithColumn("EnableSearch").AsBoolean().Nullable();

            Create.TableForModel("Profiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Cutoff").AsInt32()
                .WithColumn("Items").AsString().NotNullable();

            Create.TableForModel("QualityDefinitions")
                .WithColumn("Quality").AsInt32().Unique()
                .WithColumn("Title").AsString().Unique()
                .WithColumn("MinSize").AsDouble().Nullable()
                .WithColumn("MaxSize").AsDouble().Nullable();

            Create.TableForModel("NamingConfig")
                .WithColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true)
                .WithColumn("ArtistFolderFormat").AsString().Nullable()
                .WithColumn("RenameTracks").AsBoolean().Nullable()
                .WithColumn("StandardTrackFormat").AsString().Nullable()
                .WithColumn("AlbumFolderFormat").AsString().Nullable();

            Create.TableForModel("Blacklist")
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime()
                .WithColumn("PublishedDate").AsDateTime().Nullable()
                .WithColumn("Size").AsInt64().Nullable()
                .WithColumn("Protocol").AsInt32().Nullable()
                .WithColumn("Indexer").AsString().Nullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("TorrentInfoHash").AsString().Nullable()
                .WithColumn("Language").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("ArtistId").AsInt32().WithDefaultValue(0)
                .WithColumn("AlbumIds").AsString().WithDefaultValue("");

            Create.TableForModel("Metadata")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("MetadataFiles")
                .WithColumn("ArtistId").AsInt32().NotNullable()
                .WithColumn("Consumer").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("AlbumId").AsInt32().Nullable()
                .WithColumn("TrackFileId").AsInt32().Nullable()
                .WithColumn("Hash").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("Extension").AsString().NotNullable();

            Create.TableForModel("DownloadClients")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("PendingReleases")
                .WithColumn("Title").AsString()
                .WithColumn("Added").AsDateTime()
                .WithColumn("Release").AsString()
                .WithColumn("ArtistId").AsInt32().WithDefaultValue(0)
                .WithColumn("ParsedAlbumInfo").AsString().WithDefaultValue("");


            Create.TableForModel("RemotePathMappings")
                .WithColumn("Host").AsString()
                .WithColumn("RemotePath").AsString()
                .WithColumn("LocalPath").AsString();

            Create.TableForModel("Tags")
                .WithColumn("Label").AsString().Unique();

            Create.TableForModel("Restrictions")
                .WithColumn("Required").AsString().Nullable()
                .WithColumn("Preferred").AsString().Nullable()
                .WithColumn("Ignored").AsString().Nullable()
                .WithColumn("Tags").AsString().NotNullable();

            Create.TableForModel("DelayProfiles")
                .WithColumn("EnableUsenet").AsBoolean().NotNullable()
                .WithColumn("EnableTorrent").AsBoolean().NotNullable()
                .WithColumn("PreferredProtocol").AsInt32().NotNullable()
                .WithColumn("UsenetDelay").AsInt32().NotNullable()
                .WithColumn("TorrentDelay").AsInt32().NotNullable()
                .WithColumn("Order").AsInt32().NotNullable()
                .WithColumn("Tags").AsString().NotNullable();

            Create.TableForModel("Users")
                .WithColumn("Identifier").AsString().NotNullable().Unique()
                .WithColumn("Username").AsString().NotNullable().Unique()
                .WithColumn("Password").AsString().NotNullable();

            Create.TableForModel("Commands")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Body").AsString().NotNullable()
                .WithColumn("Priority").AsInt32().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable()
                .WithColumn("QueuedAt").AsDateTime().NotNullable()
                .WithColumn("StartedAt").AsDateTime().Nullable()
                .WithColumn("EndedAt").AsDateTime().Nullable()
                .WithColumn("Duration").AsString().Nullable()
                .WithColumn("Exception").AsString().Nullable()
                .WithColumn("Trigger").AsInt32().NotNullable();

            Create.TableForModel("IndexerStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable();

            Create.TableForModel("ExtraFiles")
                .WithColumn("ArtistId").AsInt32().NotNullable()
                .WithColumn("AlbumId").AsInt32().NotNullable()
                .WithColumn("TrackFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Create.TableForModel("LyricFiles")
                .WithColumn("ArtistId").AsInt32().NotNullable()
                .WithColumn("AlbumId").AsInt32().NotNullable()
                .WithColumn("TrackFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("Language").AsInt32().NotNullable();

            Create.TableForModel("LanguageProfiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Languages").AsString()
                .WithColumn("Cutoff").AsInt32();

            Create.TableForModel("DownloadClientStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTime().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTime().Nullable();

            Insert.IntoTable("DelayProfiles").Row(new
            {
                EnableUsenet = 1,
                EnableTorrent = 1,
                PreferredProtocol = 1,
                UsenetDelay = 0,
                TorrentDelay = 0,
                Order = int.MaxValue,
                Tags = "[]"
            });
        }

        protected override void LogDbUpgrade()
        {
            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTime().Indexed()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();
        }
    }
}
