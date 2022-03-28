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

            Create.TableForModel("Series")
                .WithColumn("TvdbId").AsInt32().Unique()
                .WithColumn("TvRageId").AsInt32()
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("Title").AsString()
                .WithColumn("TitleSlug").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("AirTime").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("SeasonFolder").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("SeriesType").AsInt32()
                .WithColumn("Network").AsString().Nullable()
                .WithColumn("UseSceneNumbering").AsBoolean()
                .WithColumn("FirstAired").AsDateTime().Nullable()
                .WithColumn("NextAiring").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Seasons").AsString().Nullable()
                .WithColumn("Actors").AsString().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("ProfileId").AsInt32().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("TvMazeId").AsInt32().WithDefaultValue(0);

            Create.TableForModel("Movies")
                .WithColumn("ImdbId").AsString().Unique()
                .WithColumn("Title").AsString()
                .WithColumn("TitleSlug").AsString().Unique()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("ProfileId").AsInt32()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("InCinemas").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("Actors").AsString().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("AddOptions").AsString().Nullable();

            Create.TableForModel("Episodes")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("EpisodeNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("EpisodeFileId").AsInt32().Nullable()
                .WithColumn("AirDate").AsString().Nullable()
                .WithColumn("AirDateUtc").AsDateTime().Nullable()
                .WithColumn("AbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneAbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneSeasonNumber").AsInt32().Nullable()
                .WithColumn("SceneEpisodeNumber").AsInt32().Nullable()
                .WithColumn("Monitored").AsBoolean().Nullable() // Nullable?
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Images").AsString().Nullable()
                .WithColumn("UnverifiedSceneNumbering").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("EpisodeFiles")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("Quality").AsString()
                .WithColumn("Size").AsInt64()
                .WithColumn("DateAdded").AsDateTime()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("MediaInfo").AsString().Nullable()
                .WithColumn("RelativePath").AsString().Nullable();

            Create.TableForModel("History")
                .WithColumn("EpisodeId").AsInt32()
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Date").AsDateTime()
                .WithColumn("Quality").AsString()
                .WithColumn("Data").AsString()
                .WithColumn("EventType").AsInt32().Nullable()
                .WithColumn("DownloadId").AsString().Nullable().Indexed();

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
                .WithColumn("Items").AsString().NotNullable()
                .WithColumn("Language").AsInt32().Nullable();

            Execute.Sql("UPDATE \"Profiles\" SET \"Language\" = 1");

            Create.TableForModel("SceneMappings")
                .WithColumn("TvdbId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32().Nullable()
                .WithColumn("SearchTerm").AsString()
                .WithColumn("ParseTerm").AsString()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Type").AsString().Nullable()
                .WithColumn("SceneSeasonNumber").AsInt32().Nullable();

            Create.TableForModel("NamingConfig")
                .WithColumn("MultiEpisodeStyle").AsInt32()
                .WithColumn("RenameEpisodes").AsBoolean().Nullable() // Set a default - Check Lidarr
                .WithColumn("StandardEpisodeFormat").AsString().Nullable()
                .WithColumn("DailyEpisodeFormat").AsString().Nullable()
                .WithColumn("SeriesFolderFormat").AsString().Nullable()
                .WithColumn("SeasonFolderFormat").AsString().Nullable()
                .WithColumn("AnimeEpisodeFormat").AsString().Nullable()
                .WithColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true);

            Create.TableForModel("Blacklist")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("EpisodeIds").AsString()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime()
                .WithColumn("PublishedDate").AsDateTime().Nullable()
                .WithColumn("Size").AsInt64().Nullable()
                .WithColumn("Protocol").AsInt32().Nullable()
                .WithColumn("Indexer").AsString().Nullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("TorrentInfoHash").AsString().Nullable();

            Create.TableForModel("QualityDefinitions")
                .WithColumn("Quality").AsInt32().Unique()
                .WithColumn("Title").AsString().Unique()
                .WithColumn("MinSize").AsDouble().Nullable()
                .WithColumn("MaxSize").AsDouble().Nullable();

            Create.TableForModel("Metadata")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("MetadataFiles")
                .WithColumn("SeriesId").AsInt32().NotNullable()
                .WithColumn("Consumer").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().Nullable()
                .WithColumn("EpisodeFileId").AsInt32().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Hash").AsString().Nullable();

            Create.TableForModel("DownloadClients")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("PendingReleases")
                .WithColumn("SeriesId").AsInt32().WithDefaultValue(0)
                .WithColumn("Title").AsString()
                .WithColumn("Added").AsDateTime()
                .WithColumn("ParsedEpisodeInfo").AsString()
                .WithColumn("Release").AsString()
                .WithColumn("MovieId").AsInt32().WithDefaultValue(0);

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

            Insert.IntoTable("DelayProfiles").Row(new
            {
                EnableUsenet = true,
                EnableTorrent = true,
                PreferredProtocol = 1,
                UsenetDelay = 0,
                TorrentDelay = 0,
                Order = int.MaxValue,
                Tags = "[]"
            });

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
                .WithColumn("IndexerId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable();

            Create.TableForModel("ExtraFiles")
                .WithColumn("SeriesId").AsInt32().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().NotNullable()
                .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Create.TableForModel("SubtitleFiles")
                .WithColumn("SeriesId").AsInt32().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().NotNullable()
                .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("Language").AsInt32().NotNullable();

            Create.Index().OnTable("History").OnColumn("Date");
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
