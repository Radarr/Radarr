using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class stevenlu_update_urlFixture : MigrationTest<stevenlu_update_url>
    {
        [Test]
        public void should_update_stevenlu_url()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportLists").Row(new
                {
                    Enabled = true,
                    EnableAuto = true,
                    Name = "StevenLu List",
                    QualityProfileId = 1,
                    MinimumAvailability = 1,
                    RootFolderPath = "/movies",
                    Monitor = 0,
                    SearchOnAdd = true,
                    Tags = "[]",
                    Implementation = "StevenLuImport",
                    ConfigContract = "StevenLuSettings",
                    Settings = new StevenLuSettings241
                    {
                        Link = "https://s3.amazonaws.com/popular-movies/movies.json"
                    }.ToJson()
                });
            });

            var items = db.Query<ImportListDefinition241>("SELECT \"Id\", \"Settings\" FROM \"ImportLists\"");

            items.Should().HaveCount(1);
            items.First().Settings.Link.Should().Be("https://popular-movies-data.stevenlu.com/movies.json");
        }
    }

    public class ImportListDefinition241 : ModelBase
    {
        public StevenLuSettings241 Settings { get; set; }
    }

    public class StevenLuSettings241
    {
        public string Link { get; set; }
    }
}
