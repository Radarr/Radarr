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

            var collections = db.Query<Collection207>("SELECT Id, Title, TmdbId, Monitored FROM Collections");

            collections.Should().HaveCount(1);
            collections.First().TmdbId.Should().Be(11);
            collections.First().Title.Should().Be("Some Collection");
            collections.First().Monitored.Should().BeFalse();

            var movies = db.Query<Movie207>("SELECT Id, CollectionId FROM Movies");

            movies.Should().HaveCount(1);
            movies.First().CollectionId.Should().Be(collections.First().Id);
        }

        [Test]
        public void should_not_duplicate_collection()
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

                c.Insert.IntoTable("Movies").Row(new
                {
                    Monitored = true,
                    Title = "Title 2",
                    CleanTitle = "CleanTitle2",
                    Status = 3,
                    MinimumAvailability = 4,
                    Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                    Recommendations = new[] { 1 }.ToJson(),
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title 2"),
                    TitleSlug = 123457,
                    TmdbId = 132457,
                    Added = DateTime.UtcNow,
                    Collection = new { Name = "Some Collection", TmdbId = 11 }.ToJson(),
                    LastInfoSync = DateTime.UtcNow,
                });
            });

            var collections = db.Query<Collection207>("SELECT Id, Title, TmdbId, Monitored FROM Collections");

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
                    Enabled = 1,
                    EnableAuto = 1,
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

            var items = db.Query<ListDefinition207>("SELECT Id, Monitor FROM ImportLists");

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
                    Enabled = 1,
                    EnableAuto = 1,
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

            var items = db.Query<ListDefinition207>("SELECT Id, Monitor FROM ImportLists");

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
                    Enabled = 1,
                    EnableAuto = 1,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = false,
                    Name = "IMDB List",
                    Implementation = "TMDbCollectionImport",
                    Settings = new TmdbCollectionListSettings206
                    {
                        CollectionId = "11"
                    }.ToJson(),
                    ConfigContract = "TMDbCollectionSettings"
                });
            });

            var items = db.Query<ListDefinition207>("SELECT Id, Monitor FROM ImportLists");

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

                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = 1,
                    EnableAuto = 1,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 4,
                    ShouldMonitor = false,
                    Name = "IMDB List",
                    Implementation = "TMDbCollectionImport",
                    Settings = new TmdbCollectionListSettings206
                    {
                        CollectionId = "11"
                    }.ToJson(),
                    ConfigContract = "TMDbCollectionSettings"
                });
            });

            var items = db.Query<Collection207>("SELECT Id, Monitored FROM Collections");

            items.Should().HaveCount(1);
            items.First().Monitored.Should().BeTrue();
        }
    }

    public class Collection207
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
    }

    public class Movie207
    {
        public int Id { get; set; }
        public int CollectionId { get; set; }
    }

    public class ListDefinition207
    {
        public int Id { get; set; }
        public int Monitor { get; set; }
    }

    public class TmdbCollectionListSettings206
    {
        public string CollectionId { get; set; }
    }
}
