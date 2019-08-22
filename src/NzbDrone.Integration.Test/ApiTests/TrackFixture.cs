using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Artist;
using System.Linq;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class TrackFixture : IntegrationTest
    {
        private ArtistResource _artist;

        [SetUp]
        public void Setup()
        {
            _artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");
        }

        [Test, Order(0)]
        public void should_be_able_to_get_all_tracks_in_artist()
        {
            Tracks.GetTracksInArtist(_artist.Id).Count.Should().BeGreaterThan(0);
        }

        [Test, Order(1)]
        public void should_be_able_to_get_a_single_track()
        {
            var tracks = Tracks.GetTracksInArtist(_artist.Id);

            Tracks.Get(tracks.First().Id).Should().NotBeNull();
        }

        [TearDown]
        public void TearDown()
        {
            Artist.Delete(_artist.Id);
            Thread.Sleep(2000);
        }
    }
}
