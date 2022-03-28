using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(181)]
    public class list_movies_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("NetImport").To("ImportLists");
            Rename.Table("NetImportStatus").To("ImportListStatus");

            Execute.Sql("UPDATE \"Config\" SET \"Key\" = 'importlistsyncinterval' WHERE \"Key\" = 'netimportsyncinterval'");

            Alter.Table("ImportLists").AddColumn("SearchOnAdd").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("ImportListMovies")
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("TmdbId").AsInt32()
                .WithColumn("ListId").AsInt32()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("InCinemas").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("Collection").AsString().Nullable()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("OriginalTitle").AsString().Nullable()
                .WithColumn("PhysicalRelease").AsDateTime().Nullable()
                .WithColumn("Translations").AsString()
                .WithColumn("Studio").AsString().Nullable()
                .WithColumn("YouTubeTrailerId").AsString().Nullable()
                .WithColumn("DigitalRelease").AsDateTime().Nullable();
        }
    }
}
