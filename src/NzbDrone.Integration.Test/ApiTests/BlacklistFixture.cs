using FluentAssertions;
using NUnit.Framework;
using Readarr.Api.V1.Artist;
using Readarr.Api.V1.Blacklist;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class BlacklistFixture : IntegrationTest
    {
        private ArtistResource _artist;

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_add_to_blacklist()
        {
            _artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");

            Blacklist.Post(new BlacklistResource
            {
                AuthorId = _artist.Id,
                SourceTitle = "Blacklist - Album 1 [2015 FLAC]"
            });
        }

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_get_all_blacklisted()
        {
            var result = Blacklist.GetPaged(0, 1000, "date", "desc");

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Records.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_remove_from_blacklist()
        {
            Blacklist.Delete(1);

            var result = Blacklist.GetPaged(0, 1000, "date", "desc");

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
        }
    }
}
