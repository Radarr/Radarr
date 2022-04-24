using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(207)]
    public class movie_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("MovieMetadata")
                .WithColumn("TmdbId").AsInt32().Unique()
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString().Nullable().Indexed()
                .WithColumn("OriginalTitle").AsString().Nullable()
                .WithColumn("CleanOriginalTitle").AsString().Nullable().Indexed()
                .WithColumn("OriginalLanguage").AsInt32()
                .WithColumn("Status").AsInt32()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("InCinemas").AsDateTime().Nullable()
                .WithColumn("PhysicalRelease").AsDateTime().Nullable()
                .WithColumn("DigitalRelease").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("SecondaryYear").AsInt32().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Recommendations").AsString()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("YouTubeTrailerId").AsString().Nullable()
                .WithColumn("Collection").AsString().Nullable()
                .WithColumn("Studio").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("Popularity").AsFloat().Nullable();

            // Transfer metadata from Movies to MovieMetadata
            Execute.Sql(@"INSERT INTO ""MovieMetadata"" (""TmdbId"", ""ImdbId"", ""Title"", ""SortTitle"", ""CleanTitle"", ""OriginalTitle"", ""CleanOriginalTitle"", ""OriginalLanguage"", ""Overview"", ""Status"", ""LastInfoSync"", ""Images"", ""Genres"", ""Ratings"", ""Runtime"", ""InCinemas"", ""PhysicalRelease"", ""DigitalRelease"", ""Year"", ""SecondaryYear"", ""Recommendations"", ""Certification"", ""YouTubeTrailerId"", ""Studio"", ""Collection"", ""Website"")
                          SELECT ""TmdbId"", ""ImdbId"", ""Title"", ""SortTitle"", ""CleanTitle"", ""OriginalTitle"", ""CleanTitle"", ""OriginalLanguage"", ""Overview"", ""Status"", ""LastInfoSync"", ""Images"", ""Genres"", ""Ratings"", ""Runtime"", ""InCinemas"", ""PhysicalRelease"", ""DigitalRelease"", ""Year"", ""SecondaryYear"", ""Recommendations"", ""Certification"", ""YouTubeTrailerId"", ""Studio"", ""Collection"", ""Website""
                          FROM ""Movies""");

            // Transfer metadata from ImportListMovies to MovieMetadata if not already in
            Execute.Sql(@"INSERT INTO ""MovieMetadata"" (""TmdbId"", ""ImdbId"", ""Title"", ""SortTitle"", ""CleanTitle"", ""OriginalTitle"", ""CleanOriginalTitle"", ""OriginalLanguage"", ""Overview"", ""Status"", ""LastInfoSync"", ""Images"", ""Genres"", ""Ratings"", ""Runtime"", ""InCinemas"", ""PhysicalRelease"", ""DigitalRelease"", ""Year"", ""Recommendations"", ""Certification"", ""YouTubeTrailerId"", ""Studio"", ""Collection"", ""Website"")
                          SELECT ""TmdbId"", ""ImdbId"", ""Title"", ""SortTitle"", ""Title"", ""OriginalTitle"", ""OriginalTitle"", 1, ""Overview"", ""Status"", ""LastInfoSync"", ""Images"", ""Genres"", ""Ratings"", ""Runtime"", ""InCinemas"", ""PhysicalRelease"", ""DigitalRelease"", ""Year"", '[]', ""Certification"", ""YouTubeTrailerId"", ""Studio"", ""Collection"", ""Website""
                          FROM ""ImportListMovies""
                          WHERE ""ImportListMovies"".""TmdbId"" NOT IN ( SELECT ""MovieMetadata"".""TmdbId"" FROM ""MovieMetadata"" )
                          AND ""ImportListMovies"".""Id"" IN ( SELECT MIN(""Id"") FROM ""ImportListMovies"" GROUP BY ""TmdbId"" )");

            // Add an MovieMetadataId column to Movies
            Alter.Table("Movies").AddColumn("MovieMetadataId").AsInt32().WithDefaultValue(0);
            Alter.Table("AlternativeTitles").AddColumn("MovieMetadataId").AsInt32().WithDefaultValue(0);
            Alter.Table("Credits").AddColumn("MovieMetadataId").AsInt32().WithDefaultValue(0);
            Alter.Table("MovieTranslations").AddColumn("MovieMetadataId").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportListMovies").AddColumn("MovieMetadataId").AsInt32().WithDefaultValue(0).Indexed();

            // Update MovieMetadataId
            Execute.Sql(@"UPDATE ""Movies""
                          SET ""MovieMetadataId"" = (SELECT ""MovieMetadata"".""Id"" 
                                                  FROM ""MovieMetadata""
                                                  WHERE ""MovieMetadata"".""TmdbId"" = ""Movies"".""TmdbId"")");

            Execute.Sql(@"UPDATE ""AlternativeTitles""
                          SET ""MovieMetadataId"" = (SELECT ""Movies"".""MovieMetadataId"" 
                                                  FROM ""Movies"" 
                                                  WHERE ""Movies"".""Id"" = ""AlternativeTitles"".""MovieId"")");

            Execute.Sql(@"UPDATE ""Credits""
                          SET ""MovieMetadataId"" = (SELECT ""Movies"".""MovieMetadataId"" 
                                                  FROM ""Movies"" 
                                                  WHERE ""Movies"".""Id"" = ""Credits"".""MovieId"")");

            Execute.Sql(@"UPDATE ""MovieTranslations""
                          SET ""MovieMetadataId"" = (SELECT ""Movies"".""MovieMetadataId"" 
                                                  FROM ""Movies"" 
                                                  WHERE ""Movies"".""Id"" = ""MovieTranslations"".""MovieId"")");

            Execute.Sql(@"UPDATE ""ImportListMovies""
                          SET ""MovieMetadataId"" = (SELECT ""MovieMetadata"".""Id"" 
                                                  FROM ""MovieMetadata"" 
                                                  WHERE ""MovieMetadata"".""TmdbId"" = ""ImportListMovies"".""TmdbId"")");

            // Alter MovieMetadataId column to be unique on Movies
            Alter.Table("Movies").AlterColumn("MovieMetadataId").AsInt32().Unique();

            // Remove Movie Link from Metadata Tables
            Delete.Column("MovieId").FromTable("AlternativeTitles");
            Delete.Column("MovieId").FromTable("Credits");
            Delete.Column("MovieId").FromTable("MovieTranslations");

            // Remove the columns in Movies now in MovieMetadata
            Delete.Column("TmdbId")
                .Column("ImdbId")
                .Column("Title")
                .Column("SortTitle")
                .Column("CleanTitle")
                .Column("OriginalTitle")
                .Column("OriginalLanguage")
                .Column("Overview")
                .Column("Status")
                .Column("LastInfoSync")
                .Column("Images")
                .Column("Genres")
                .Column("Ratings")
                .Column("Runtime")
                .Column("InCinemas")
                .Column("PhysicalRelease")
                .Column("DigitalRelease")
                .Column("Year")
                .Column("SecondaryYear")
                .Column("Recommendations")
                .Column("Certification")
                .Column("YouTubeTrailerId")
                .Column("Studio")
                .Column("Collection")
                .Column("Website")

                // as well as the ones no longer used
                .Column("LastDiskSync")
                .Column("TitleSlug")
                .FromTable("Movies");

            // Remove the columns in ImportListMovies now in MovieMetadata
            Delete.Column("TmdbId")
                .Column("ImdbId")
                .Column("Title")
                .Column("SortTitle")
                .Column("Overview")
                .Column("Status")
                .Column("LastInfoSync")
                .Column("OriginalTitle")
                .Column("Translations")
                .Column("Images")
                .Column("Genres")
                .Column("Ratings")
                .Column("Runtime")
                .Column("InCinemas")
                .Column("PhysicalRelease")
                .Column("DigitalRelease")
                .Column("Year")
                .Column("Certification")
                .Column("YouTubeTrailerId")
                .Column("Studio")
                .Column("Collection")
                .Column("Website")
                .FromTable("ImportListMovies");
        }
    }
}
