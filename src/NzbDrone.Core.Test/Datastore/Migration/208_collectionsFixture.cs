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
    public class collectionsFixture : MigrationTest<collections>
    {
        [Test]
        public void should_add_collection_from_movie_and_link_back_to_movie()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    MinimumAvailability = 4,
                    ProfileId = 1,
                    MovieFileId = 0,
                    MovieMetadataId = 1,
                    Path = string.Format("/Movies/{0}", "Title"),
                });

                c.Insert.IntoTable("MovieMetadata").Row(new
                {
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalTitle = "Title",
                    CleanOriginalTitle = "CleanTitle",
                    OriginalLanguage = 1,
                    TmdbId = 132456,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });
            });

            var collections = db.Query<Collection208>("SELECT \"Id\", \"Title\", \"TmdbId\", \"Monitored\" FROM \"Collections\"");

            collections.Should().HaveCount(1);
            collections.First().TmdbId.Should().Be(11);
            collections.First().Title.Should().Be("Some Collection");
            collections.First().Monitored.Should().BeFalse();

            var movies = db.Query<Movie208>("SELECT \"Id\", \"CollectionTmdbId\" FROM \"MovieMetadata\"");

            movies.Should().HaveCount(1);
            movies.First().CollectionTmdbId.Should().Be(collections.First().TmdbId);
        }

        [Test]
        public void should_not_duplicate_collection()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    MinimumAvailability = 4,
                    ProfileId = 1,
                    MovieFileId = 0,
                    MovieMetadataId = 1,
                    Path = string.Format("/Movies/{0}", "Title"),
                });

                c.Insert.IntoTable("MovieMetadata").Row(new
                {
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalTitle = "Title",
                    CleanOriginalTitle = "CleanTitle",
                    OriginalLanguage = 1,
                    TmdbId = 132456,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    MinimumAvailability = 4,
                    ProfileId = 1,
                    MovieFileId = 0,
                    MovieMetadataId = 2,
                    Path = string.Format("/Movies/{0}", "Title"),
                });

                c.Insert.IntoTable("MovieMetadata").Row(new
                {
                    Title = "Title2",
                    CleanTitle = "CleanTitle2",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalTitle = "Title2",
                    CleanOriginalTitle = "CleanTitle2",
                    OriginalLanguage = 1,
                    TmdbId = 132457,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });
            });

            var collections = db.Query<Collection208>("SELECT \"Id\", \"Title\", \"TmdbId\", \"Monitored\" FROM \"Collections\"");

            collections.Should().HaveCount(1);
            collections.First().TmdbId.Should().Be(11);
            collections.First().Title.Should().Be("Some Collection");
            collections.First().Monitored.Should().BeFalse();
        }

        [Test]
        public void should_migrate_true_monitor_setting_on_lists()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = true,
                    Name = "IMDB List",
                    Implementation = "RadarrLists",
                    Settings = new RadarrListSettings169
                    {
                        APIURL = "https://api.radarr.video/v2",
                        Path = "/imdb/list?listId=ls000199717",
                    }.ToJson(),
                    ConfigContract = "RadarrSettings"
                });
            });

            var items = db.Query<ListDefinition208>("SELECT \"Id\", \"Monitor\" FROM \"ImportLists\"");

            items.Should().HaveCount(1);
            items.First().Monitor.Should().Be(0);
        }

        [Test]
        public void should_migrate_false_monitor_setting_on_lists()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = false,
                    Name = "IMDB List",
                    Implementation = "RadarrLists",
                    Settings = new RadarrListSettings169
                    {
                        APIURL = "https://api.radarr.video/v2",
                        Path = "/imdb/list?listId=ls000199717",
                    }.ToJson(),
                    ConfigContract = "RadarrSettings"
                });
            });

            var items = db.Query<ListDefinition208>("SELECT \"Id\", \"Monitor\" FROM \"ImportLists\"");

            items.Should().HaveCount(1);
            items.First().Monitor.Should().Be(2);
        }

        [Test]
        public void should_purge_tmdb_collection_lists()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = false,
                    Name = "IMDB List",
                    Implementation = "TMDbCollectionImport",
                    Settings = new TmdbCollectionListSettings207
                    {
                        CollectionId = "11"
                    }.ToJson(),
                    ConfigContract = "TMDbCollectionSettings"
                });
            });

            var items = db.Query<ListDefinition208>("SELECT \"Id\", \"Monitor\" FROM \"ImportLists\"");

            items.Should().HaveCount(0);
        }

        [Test]
        public void should_monitor_new_collection_if_list_enabled_and_auto()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    MinimumAvailability = 4,
                    ProfileId = 1,
                    MovieFileId = 0,
                    MovieMetadataId = 1,
                    Path = string.Format("/Movies/{0}", "Title"),
                });

                c.Insert.IntoTable("MovieMetadata").Row(new
                {
                    Title = "Title",
                    CleanTitle = "CleanTitle",
                    Status = 3,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalTitle = "Title",
                    CleanOriginalTitle = "CleanTitle",
                    OriginalLanguage = 1,
                    TmdbId = 132456,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });

                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = false,
                    Name = "IMDB List",
                    Implementation = "TMDbCollectionImport",
                    Settings = new TmdbCollectionListSettings207
                    {
                        CollectionId = "11"
                    }.ToJson(),
                    ConfigContract = "TMDbCollectionSettings"
                });
            });

            var items = db.Query<Collection208>("SELECT \"Id\", \"Monitored\" FROM \"Collections\"");

            items.Should().HaveCount(1);
            items.First().Monitored.Should().BeTrue();
        }
    }

    public class Collection208
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
    }

    public class Movie208
    {
        public int Id { get; set; }
        public int CollectionTmdbId { get; set; }
    }

    public class ListDefinition208
    {
        public int Id { get; set; }
        public int Monitor { get; set; }
    }

    public class TmdbCollectionListSettings207
    {
        public string CollectionId { get; set; }
    }
}
