using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Artist;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class TrackFixture : IntegrationTest
    {
        private ArtistResource _artist;

        [SetUp]
        public void Setup()
        {
            _artist = GivenArtistWithTracks();
        }

        private ArtistResource GivenArtistWithTracks()
        {
            var newArtist = Artist.Lookup("archer").Single(c => c.ForeignArtistId == "110381");

            newArtist.QualityProfileId = 1;
            newArtist.Path = @"C:\Test\Archer".AsOsAgnostic();

            newArtist = Artist.Post(newArtist);

            WaitForCompletion(() => Tracks.GetTracksInArtist(newArtist.Id).Count > 0);

            return newArtist;
        }

        [Test]
        public void should_be_able_to_get_all_tracks_in_artist()
        {
            Tracks.GetTracksInArtist(_artist.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_track()
        {
            var tracks = Tracks.GetTracksInArtist(_artist.Id);

            Tracks.Get(tracks.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status()
        {
            var tracks = Tracks.GetTracksInArtist(_artist.Id);
            var updatedTrack = tracks.First();
            updatedTrack.Monitored = false;

            Tracks.Put(updatedTrack).Monitored.Should().BeFalse();
        }


        [TearDown]
        public void TearDown()
        {
            Artist.Delete(_artist.Id);
            Thread.Sleep(2000);
        }
    }
}
