using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(161)]
    public class speed_improvements : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Auto indices SQLite is creating
            Create.Index("IX_MovieFiles_MovieId").OnTable("MovieFiles").OnColumn("MovieId");
            Create.Index("IX_AlternativeTitles_MovieId").OnTable("AlternativeTitles").OnColumn("MovieId");

            // Speed up release processing (these are present in Sonarr)
            Create.Index("IX_Movies_CleanTitle").OnTable("Movies").OnColumn("CleanTitle");
            Create.Index("IX_Movies_ImdbId").OnTable("Movies").OnColumn("ImdbId");
            Create.Index("IX_Movies_TmdbId").OnTable("Movies").OnColumn("TmdbId");
        }
    }
}
