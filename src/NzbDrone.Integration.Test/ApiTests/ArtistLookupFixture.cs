using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistLookupFixture : IntegrationTest
    {
        [TestCase("Robert Harris", "Robert Harris")]
        [TestCase("J.K. Rowling", "J.K. Rowling")]
        public void lookup_new_artist_by_name(string term, string name)
        {
            var artist = Artist.Lookup(term);

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.ArtistName == name);
        }

        [Test]
        public void lookup_new_artist_by_goodreads_book_id()
        {
            var artist = Artist.Lookup("readarr:1");

            artist.Should().NotBeEmpty();
            artist.Should().Contain(c => c.ArtistName == "J.K. Rowling");
        }
    }
}
