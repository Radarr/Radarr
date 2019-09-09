using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistLookupFixture : IntegrationTest
    {
        [TestCase("Kiss", "Kiss")]
        [TestCase("Linkin Park", "Linkin Park")]
        public void lookup_new_artist_by_name(string term, string name)
        {
            var artist = Artist.Lookup(term);

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.ArtistName == name);
        }

        [Test]
        public void lookup_new_artist_by_mbid()
        {
            var artist = Artist.Lookup("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419");

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.ArtistName == "Linkin Park");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_artist_using_asterix()
        {
            var artist = Artist.Lookup("*");

            artist.Should().NotBeEmpty();
        }
    }
}
