using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Blacklist;

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
            _artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");
            
            Blacklist.Post(new BlacklistResource
            {
                ArtistId = _artist.Id,
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
