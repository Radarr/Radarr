using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(252)]
    public class add_tv_tables : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("TVShows")
                .WithColumn("TvdbId").AsInt32().Nullable()
                .WithColumn("TmdbId").AsInt32().Nullable()
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("AniDbId").AsInt32().Nullable()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Network").AsString().Nullable()
                .WithColumn("Status").AsInt32().NotNullable()
                .WithColumn("Runtime").AsInt32().Nullable()
                .WithColumn("AirTime").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("FirstAired").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().NotNullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("OriginalLanguage").AsString().Nullable()
                .WithColumn("IsAnime").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("SeriesType").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("UseSceneNumbering").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("QualityProfileId").AsInt32().NotNullable()
                .WithColumn("SeasonFolder").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("Monitored").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("MonitorNewItems").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastSearchTime").AsDateTime().Nullable();

            Create.Index("IX_TVShows_TvdbId").OnTable("TVShows").OnColumn("TvdbId");
            Create.Index("IX_TVShows_Path").OnTable("TVShows").OnColumn("Path");
            Create.Index("IX_TVShows_Monitored").OnTable("TVShows").OnColumn("Monitored");

            Create.TableForModel("Seasons")
                .WithColumn("TVShowId").AsInt32().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().NotNullable()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Monitored").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.Index("IX_Seasons_TVShowId").OnTable("Seasons").OnColumn("TVShowId");
            Create.Index("IX_Seasons_TVShowId_SeasonNumber").OnTable("Seasons")
                .OnColumn("TVShowId").Ascending()
                .OnColumn("SeasonNumber").Ascending();

            Create.TableForModel("Episodes")
                .WithColumn("TVShowId").AsInt32().NotNullable()
                .WithColumn("SeasonId").AsInt32().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().NotNullable()
                .WithColumn("EpisodeNumber").AsInt32().NotNullable()
                .WithColumn("AbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneSeasonNumber").AsInt32().Nullable()
                .WithColumn("SceneEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneAbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("AirDate").AsDateTime().Nullable()
                .WithColumn("AirDateUtc").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32().Nullable()
                .WithColumn("UnverifiedSceneNumbering").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsSpecial").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("EpisodeFileId").AsInt32().Nullable()
                .WithColumn("MediaType").AsInt32().NotNullable().WithDefaultValue(2)
                .WithColumn("Monitored").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("QualityProfileId").AsInt32().NotNullable()
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("LastSearchTime").AsDateTime().Nullable()
                .WithColumn("AuthorId").AsInt32().Nullable()
                .WithColumn("BookSeriesId").AsInt32().Nullable();

            Create.Index("IX_Episodes_TVShowId").OnTable("Episodes").OnColumn("TVShowId");
            Create.Index("IX_Episodes_SeasonId").OnTable("Episodes").OnColumn("SeasonId");
            Create.Index("IX_Episodes_TVShowId_SeasonNumber_EpisodeNumber").OnTable("Episodes")
                .OnColumn("TVShowId").Ascending()
                .OnColumn("SeasonNumber").Ascending()
                .OnColumn("EpisodeNumber").Ascending();
            Create.Index("IX_Episodes_TVShowId_AbsoluteEpisodeNumber").OnTable("Episodes")
                .OnColumn("TVShowId").Ascending()
                .OnColumn("AbsoluteEpisodeNumber").Ascending();
            Create.Index("IX_Episodes_Monitored").OnTable("Episodes").OnColumn("Monitored");

            Create.TableForModel("EpisodeFiles")
                .WithColumn("TVShowId").AsInt32().Nullable()
                .WithColumn("SeasonId").AsInt32().Nullable()
                .WithColumn("EpisodeId").AsInt32().Nullable()
                .WithColumn("RelativePath").AsString().Nullable()
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("Size").AsInt64().NotNullable()
                .WithColumn("DateAdded").AsDateTime().NotNullable()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("Quality").AsString().Nullable()
                .WithColumn("Language").AsString().Nullable()
                .WithColumn("StreamingSource").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("MediaInfo").AsString().Nullable();

            Create.Index("IX_EpisodeFiles_TVShowId").OnTable("EpisodeFiles").OnColumn("TVShowId");
            Create.Index("IX_EpisodeFiles_SeasonId").OnTable("EpisodeFiles").OnColumn("SeasonId");
            Create.Index("IX_EpisodeFiles_EpisodeId").OnTable("EpisodeFiles").OnColumn("EpisodeId");
        }
    }
}
