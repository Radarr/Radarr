using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_various_qualites_in_profileFixture : MigrationTest<add_various_qualites_in_profile>
    {
        private string GenerateQualityJson(int quality, bool allowed)
        {
            return $"{{ \"quality\": {quality}, \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
        }

        [Test]
        public void should_add_wav_quality()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "Lossless",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.MP3_320, false)}, {GenerateQualityJson((int)Quality.FLAC, true)}]"
                });
            });

            var profiles = db.Query<Profile4>("SELECT Items FROM Profiles LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(7);
            items.Select(v => v.Quality).Should().Contain(13);
            items.Select(v => v.Items.Count).Should().BeEquivalentTo(9, 5, 6, 3, 0, 5, 5);
            items.Select(v => v.Allowed).Should().BeEquivalentTo(false, true, false, true, false, false, false);
        }

        [Test]
        public void should_add_trash_lossy_quality_group_and_qualities()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "Lossless",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.MP3_320, false)}, {GenerateQualityJson((int)Quality.FLAC, true)}]"
                });
            });

            var profiles = db.Query<Profile4>("SELECT Items FROM Profiles LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(7);
            items.Select(v => v.Name).Should().Contain("Trash Quality Lossy");
            items.Select(v => v.Items.Count).Should().BeEquivalentTo(9, 5, 6, 3, 0, 5, 5);
            items.Select(v => v.Allowed).Should().BeEquivalentTo(false, true, false, true, false, false, false);
        }
    }
}
