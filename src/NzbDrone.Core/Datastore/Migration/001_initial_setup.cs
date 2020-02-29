using System;
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
                .WithColumn("Path").AsString().Unique()
                .WithColumn("Name").AsString().Nullable()
                .WithColumn("DefaultMetadataProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("DefaultQualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("DefaultMonitorOption").AsInt32().WithDefaultValue(0)
                .WithColumn("DefaultTags").AsString().Nullable();

            Create.TableForModel("Artists")
                .WithColumn("CleanName").AsString().Indexed()
                .WithColumn("Path").AsString().Indexed()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("AlbumFolder").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("SortName").AsString().Nullable()
                .WithColumn("QualityProfileId").AsInt32().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("MetadataProfileId").AsInt32().WithDefaultValue(1)
                .WithColumn("ArtistMetadataId").AsInt32().Unique();

            Create.TableForModel("ArtistMetadata")
                .WithColumn("ForeignArtistId").AsString().Unique()
                .WithColumn("Name").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("Type").AsString().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Images").AsString()
                .WithColumn("Links").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Members").AsString().Nullable()
                .WithColumn("Aliases").AsString().WithDefaultValue("[]")
                .WithColumn("OldForeignArtistIds").AsString().WithDefaultValue("[]");

            Create.TableForModel("Albums")
                .WithColumn("ForeignAlbumId").AsString().Unique()
                .WithColumn("Title").AsString()
                .WithColumn("CleanTitle").AsString().Indexed()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("ProfileId").AsInt32().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("AlbumType").AsString()
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("SecondaryTypes").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("ArtistMetadataId").AsInt32().WithDefaultValue(0)
                .WithColumn("AnyReleaseOk").AsBoolean().WithDefaultValue(true)
                .WithColumn("Links").AsString().Nullable()
                .WithColumn("OldForeignAlbumIds").AsString().WithDefaultValue("[]");

            Create.TableForModel("AlbumReleases")
                .WithColumn("ForeignReleaseId").AsString().Unique()
                .WithColumn("AlbumId").AsInt32().Indexed()
                .WithColumn("Title").AsString()
                .WithColumn("Status").AsString()
                .WithColumn("Duration").AsInt32().WithDefaultValue(0)
                .WithColumn("Label").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("Country").AsString().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Media").AsString().Nullable()
                .WithColumn("TrackCount").AsInt32().Nullable()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("OldForeignReleaseIds").AsString().WithDefaultValue("[]");

            Create.TableForModel("Tracks")
                .WithColumn("ForeignTrackId").AsString().Unique()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Explicit").AsBoolean()
                .WithColumn("TrackFileId").AsInt32().Nullable().Indexed()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Duration").AsInt32().WithDefaultValue(0)
                .WithColumn("MediumNumber").AsInt32().WithDefaultValue(0)
                .WithColumn("AbsoluteTrackNumber").AsInt32().WithDefaultValue(0)
                .WithColumn("TrackNumber").AsString().Nullable()
                .WithColumn("ForeignRecordingId").AsString().WithDefaultValue("0")
                .WithColumn("AlbumReleaseId").AsInt32().WithDefaultValue(0)
                .WithColumn("ArtistMetadataId").AsInt32().WithDefaultValue(0)
                .WithColumn("OldForeignRecordingIds").AsString().WithDefaultValue("[]")
                .WithColumn("OldForeignTrackIds").AsString().WithDefaultValue("[]");

            Create.Index().OnTable("Tracks").OnColumn("ArtistId").Ascending()
                                            .OnColumn("AlbumId").Ascending()
                                            .OnColumn("TrackNumber").Ascending();

            Create.TableForModel("TrackFiles")
                .WithColumn("AlbumId").AsInt32().Indexed()
                .WithColumn("Quality").AsString()
                .WithColumn("Size").AsInt64()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("DateAdded").AsDateTime()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("MediaInfo").AsString().Nullable()
                .WithColumn("Modified").AsDateTime().WithDefaultValue(new DateTime(2000, 1, 1))
                .WithColumn("Path").AsString().NotNullable().Unique();

            Create.TableForModel("History")
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Date").AsDateTime().Indexed()
                .WithColumn("Quality").AsString()
                .WithColumn("Data").AsString()
                .WithColumn("EventType").AsInt32().Nullable().Indexed()
                .WithColumn("DownloadId").AsString().Nullable().Indexed()
                .WithColumn("ArtistId").AsInt32().WithDefaultValue(0)
                .WithColumn("AlbumId").AsInt32().Indexed().WithDefaultValue(0)
                .WithColumn("TrackId").AsInt32().WithDefaultValue(0);

            Create.TableForModel("Notifications")
                .WithColumn("Name").AsString()
                .WithColumn("OnGrab").AsBoolean()
                .WithColumn("Settings").AsString()
                .WithColumn("Implementation").AsString()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("OnUpgrade").AsBoolean().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("OnRename").AsBoolean().NotNullable()
                .WithColumn("OnReleaseImport").AsBoolean().WithDefaultValue(0)
                .WithColumn("OnHealthIssue").AsBoolean().WithDefaultValue(0)
                .WithColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(0)
                .WithColumn("OnDownloadFailure").AsBoolean().WithDefaultValue(0)
                .WithColumn("OnImportFailure").AsBoolean().WithDefaultValue(0)
                .WithColumn("OnTrackRetag").AsBoolean().WithDefaultValue(0);

            Create.TableForModel("ScheduledTasks")
                .WithColumn("TypeName").AsString().Unique()
                .WithColumn("Interval").AsInt32()
                .WithColumn("LastExecution").AsDateTime()
                .WithColumn("LastStartTime").AsDateTime().Nullable();

            Create.TableForModel("Indexers")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("EnableRss").AsBoolean().Nullable()
                .WithColumn("EnableAutomaticSearch").AsBoolean().Nullable()
                .WithColumn("EnableInteractiveSearch").AsBoolean().NotNullable();

            Create.TableForModel("QualityProfiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Cutoff").AsInt32()
                .WithColumn("Items").AsString().NotNullable()
                .WithColumn("UpgradeAllowed").AsInt32().Nullable();

            Create.TableForModel("MetadataProfiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("PrimaryAlbumTypes").AsString()
                .WithColumn("SecondaryAlbumTypes").AsString()
                .WithColumn("ReleaseStatuses").AsString().WithDefaultValue("");

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
                .WithColumn("AlbumFolderFormat").AsString().Nullable()
                .WithColumn("MultiDiscTrackFormat").AsString().Nullable();

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
                .WithColumn("ConfigContract").AsString().NotNullable()
                .WithColumn("Priority").AsInt32().WithDefaultValue(1);

            Create.TableForModel("PendingReleases")
                .WithColumn("Title").AsString()
                .WithColumn("Added").AsDateTime()
                .WithColumn("Release").AsString()
                .WithColumn("ArtistId").AsInt32().WithDefaultValue(0)
                .WithColumn("ParsedAlbumInfo").AsString().WithDefaultValue("")
                .WithColumn("Reason").AsInt32().WithDefaultValue(0);

            Create.TableForModel("RemotePathMappings")
                .WithColumn("Host").AsString()
                .WithColumn("RemotePath").AsString()
                .WithColumn("LocalPath").AsString();

            Create.TableForModel("Tags")
                .WithColumn("Label").AsString().Unique();

            Create.TableForModel("ReleaseProfiles")
                .WithColumn("Required").AsString().Nullable()
                .WithColumn("Preferred").AsString().Nullable()
                .WithColumn("Ignored").AsString().Nullable()
                .WithColumn("Tags").AsString().NotNullable()
                .WithColumn("IncludePreferredWhenRenaming").AsBoolean().WithDefaultValue(true);

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
                .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Create.TableForModel("DownloadClientStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable();

            Create.TableForModel("ImportLists")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("EnableAutomaticAdd").AsBoolean().Nullable()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("ShouldMonitor").AsInt32()
                .WithColumn("ProfileId").AsInt32()
                .WithColumn("MetadataProfileId").AsInt32()
                .WithColumn("Tags").AsString().Nullable();

            Create.TableForModel("ImportListStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastSyncListInfo").AsString().Nullable();

            Create.TableForModel("ImportListExclusions")
                .WithColumn("ForeignId").AsString().NotNullable().Unique()
                .WithColumn("Name").AsString().NotNullable();

            Create.TableForModel("CustomFilters")
                .WithColumn("Type").AsString().NotNullable()
                .WithColumn("Label").AsString().NotNullable()
                .WithColumn("Filters").AsString().NotNullable();

            Create.Index().OnTable("Albums").OnColumn("ArtistId");
            Create.Index().OnTable("Albums").OnColumn("ArtistId").Ascending()
                                            .OnColumn("ReleaseDate").Ascending();

            Delete.Index().OnTable("History").OnColumn("AlbumId");
            Create.Index().OnTable("History").OnColumn("AlbumId").Ascending()
                                             .OnColumn("Date").Descending();

            Delete.Index().OnTable("History").OnColumn("DownloadId");
            Create.Index().OnTable("History").OnColumn("DownloadId").Ascending()
                                             .OnColumn("Date").Descending();

            Create.Index().OnTable("Artists").OnColumn("Monitored").Ascending();
            Create.Index().OnTable("Albums").OnColumn("ArtistMetadataId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("ArtistMetadataId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("AlbumReleaseId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("ForeignRecordingId").Ascending();

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
