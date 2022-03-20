using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class movie_metadataFixture : MigrationTest<movie_metadata>
    {
        [Test]
        public void should_add_metadata_from_movie_and_link_back_to_movie()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 132456,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(132456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"Movies\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }

        [Test]
        public void should_link_metadata_to_credits()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Id = 5,
                    Monitored = true,
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 132456,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("Credits").Row(new
                {
                    MovieId = 5,
                    CreditTmdbId = 123,
                    PersonTmdbId = 456,
                    Order = 1,
                    Type = 1,
                    Name = "Some Person",
                    Images = new[] { new { CoverType = "Poster" } }.ToJson()
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(132456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"Credits\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }

        [Test]
        public void should_link_metadata_to_alt_title()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Id = 5,
                    Monitored = true,
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 132456,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("AlternativeTitles").Row(new
                {
                    MovieId = 5,
                    Title = "Some Alt",
                    CleanTitle = "somealt",
                    SourceType = 1,
                    SourceId = 1,
                    Votes = 0,
                    VoteCount = 0,
                    Language = 1
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(132456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"AlternativeTitles\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }

        [Test]
        public void should_link_metadata_to_translation()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Id = 5,
                    Monitored = true,
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 132456,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("MovieTranslations").Row(new
                {
                    MovieId = 5,
                    Title = "Some Trans",
                    Language = 1
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(132456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"MovieTranslations\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }

        [Test]
        public void should_add_metadata_from_list_and_link_back()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportListMovies").Row(new
                {
                    Title = "Title",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Runtime = 90,
                    TmdbId = 123456,
                    ListId = 4,
                    Translations = new[] { new { } }.ToJson(),
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(123456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"ImportListMovies\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }

        [Test]
        public void should_not_duplicate_metadata()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 123456,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("ImportListMovies").Row(new
                {
                    Title = "Title",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Runtime = 90,
                    TmdbId = 123456,
                    ListId = 4,
                    Translations = new[] { new { } }.ToJson(),
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                });
            });

            var metadata = db.Query<MovieMetadata207>("SELECT \"Id\", \"Title\", \"TmdbId\" FROM \"MovieMetadata\"");

            metadata.Should().HaveCount(1);
            metadata.First().TmdbId.Should().Be(123456);
            metadata.First().Title.Should().Be("Title");

            var movies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"Movies\"");

            movies.Should().HaveCount(1);
            movies.First().MovieMetadataId.Should().Be(metadata.First().Id);

            var listMovies = db.Query<Movie207>("SELECT \"Id\", \"MovieMetadataId\" FROM \"ImportListMovies\"");

            listMovies.Should().HaveCount(1);
            listMovies.First().MovieMetadataId.Should().Be(metadata.First().Id);
        }
    }

    public class MovieMetadata207
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
    }

    public class Movie207
    {
        public int Id { get; set; }
        public int MovieMetadataId { get; set; }
    }
}
