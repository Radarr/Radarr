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
    public class fix_trakt_list_configFixture : MigrationTest<fix_trakt_list_config>
    {
        [Test]
        public void should_change_implementation_contract_on_radarr_lists()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NetImport").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 1,
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

            var items = db.Query<ListDefinition169>("SELECT * FROM \"NetImport\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("RadarrListImport");
            items.First().ConfigContract.Should().Be("RadarrListSettings");
            items.First().Settings.Count.Should().Be(2);
            items.First().Settings.First.Should().NotBeEmpty();
        }

        [Test]
        public void should_change_implementation_contract_type_on_trakt_user()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NetImport").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 1,
                    ShouldMonitor = true,
                    Name = "TraktImport",
                    Implementation = "TraktImport",
                    Settings = new TraktSettings169
                    {
                        AccessToken = "123456798",
                        RefreshToken = "987654321",
                        Rating = "0-100",
                        TraktListType = (int)TraktListType169.UserWatchList,
                        Username = "someuser",
                    }.ToJson(),
                    ConfigContract = "TraktSettings"
                });
            });

            var items = db.Query<ListDefinition169>("SELECT * FROM \"NetImport\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("TraktUserImport");
            items.First().ConfigContract.Should().Be("TraktUserSettings");

            var firstSettings = items.First().Settings.ToObject<TraktUserSettings170>();
            firstSettings.AccessToken.Should().NotBeEmpty();
            firstSettings.TraktListType.Should().Be((int)TraktUserListType170.UserWatchList);
        }

        [Test]
        public void should_change_implementation_contract_type_on_trakt_popular()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NetImport").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 1,
                    ShouldMonitor = true,
                    Name = "TraktImport",
                    Implementation = "TraktImport",
                    Settings = new TraktSettings169
                    {
                        AccessToken = "123456798",
                        RefreshToken = "987654321",
                        Rating = "0-100",
                        TraktListType = (int)TraktListType169.Popular,
                        Username = "someuser",
                    }.ToJson(),
                    ConfigContract = "TraktSettings"
                });
            });

            var items = db.Query<ListDefinition169>("SELECT * FROM \"NetImport\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("TraktPopularImport");
            items.First().ConfigContract.Should().Be("TraktPopularSettings");

            var firstSettings = items.First().Settings.ToObject<TraktPopularSettings170>();
            firstSettings.AccessToken.Should().NotBeEmpty();
            firstSettings.TraktListType.Should().Be((int)TraktPopularListType170.Popular);
        }

        [Test]
        public void should_change_implementation_contract_type_on_trakt_list()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NetImport").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    RootFolderPath = "D:\\Movies",
                    ProfileId = 1,
                    MinimumAvailability = 1,
                    ShouldMonitor = true,
                    Name = "TraktImport",
                    Implementation = "TraktImport",
                    Settings = new TraktSettings169
                    {
                        AccessToken = "123456798",
                        RefreshToken = "987654321",
                        Rating = "0-100",
                        TraktListType = (int)TraktListType169.UserCustomList,
                        Username = "someuser",
                        Listname = "mylist"
                    }.ToJson(),
                    ConfigContract = "TraktSettings"
                });
            });

            var items = db.Query<ListDefinition169>("SELECT * FROM \"NetImport\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("TraktListImport");
            items.First().ConfigContract.Should().Be("TraktListSettings");

            var firstSettings = items.First().Settings.ToObject<TraktListSettings170>();
            firstSettings.AccessToken.Should().NotBeEmpty();
            firstSettings.Listname.Should().Be("mylist");
        }
    }

    public class ListDefinition169
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public string Name { get; set; }
        public string Implementation { get; set; }
        public JObject Settings { get; set; }
        public string ConfigContract { get; set; }
        public string RootFolderPath { get; set; }
        public int ProfileId { get; set; }
        public int MinimumAvailability { get; set; }
        public List<int> Tags { get; set; }
    }

    public class RadarrListSettings169
    {
        public string APIURL { get; set; }
        public string Path { get; set; }
    }
}
