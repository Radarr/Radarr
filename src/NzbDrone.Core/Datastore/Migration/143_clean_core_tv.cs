using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(143)]
    public class clean_core_tv : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("Episodes");
            Delete.Table("EpisodeFiles");
            Delete.Table("Series");
            Delete.Table("SceneMappings");

            Delete.Column("SeriesId")
                  .Column("EpisodeIds")
                  .FromTable("Blacklist");

            Delete.Column("SeriesId")
                  .Column("EpisodeId")
                  .FromTable("History");

            Delete.Column("StandardEpisodeFormat")
                  .Column("DailyEpisodeFormat")
                  .Column("AnimeEpisodeFormat")
                  .Column("SeasonFolderFormat")
                  .Column("SeriesFolderFormat")
                  .FromTable("NamingConfig");

            Delete.Column("SeriesId")
                  .Column("ParsedEpisodeInfo")
                  .FromTable("PendingReleases");
        }
    }
}
