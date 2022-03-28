using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class remove_predbFixture : MigrationTest<remove_predb>
    {
        [Test]
        public void should_change_min_avail_from_predb_on_list()
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

            var items = db.Query<ListDefinition201>("SELECT \"Id\", \"MinimumAvailability\" FROM \"ImportLists\"");

            items.Should().HaveCount(1);
            items.First().MinimumAvailability.Should().Be(3);
        }

        [Test]
        public void should_change_min_avail_from_predb_on_movie()
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
                    HasPreDBEntry = false,
                    Runtime = 90,
                    OriginalLanguage = 1,
                    ProfileId = 1,
                    MovieFileId = 0,
                    Path = string.Format("/Movies/{0}", "Title"),
                    TitleSlug = 123456,
                    TmdbId = 132456,
                    Added = DateTime.UtcNow,
                    LastInfoSync = DateTime.UtcNow,
                });
            });

            var items = db.Query<Movie201>("SELECT \"Id\", \"MinimumAvailability\" FROM \"Movies\"");

            items.Should().HaveCount(1);
            items.First().MinimumAvailability.Should().Be(3);
        }
    }

    public class ListDefinition201
    {
        public int Id { get; set; }
        public int MinimumAvailability { get; set; }
    }

    public class Movie201
    {
        public int Id { get; set; }
        public int MinimumAvailability { get; set; }
    }
}
