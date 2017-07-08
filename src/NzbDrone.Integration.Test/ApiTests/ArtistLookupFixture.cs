using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistLookupFixture : IntegrationTest
    {
        [TestCase("archer", "Archer (2009)")]
        [TestCase("90210", "90210")]
        public void lookup_new_artist_by_name(string term, string name)
        {
            var artist = Artist.Lookup(term);

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.Name == name);
        }

        [Test]
        public void lookup_new_series_by_tvdbid()
        {
            var artist = Artist.Lookup("lidarr:266189");

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.Name == "The Blacklist");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_series_using_asterix()
        {
            var artist = Artist.Lookup("*");

            artist.Should().NotBeEmpty();
        }
    }
}