using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class WantedFixture : IntegrationTest
    {
        [Test, Order(0)]
        public void missing_should_be_empty()
        {
            EnsureNoArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_have_artist()
        {
            EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Artist.Should().NotBeNull();
            result.Records.First().Artist.ArtistName.Should().Be("Alien Ant Farm");
        }

        [Test, Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, "Lossless");
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", true);
            EnsureTrackFile(artist, 1, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, "Lossless");
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", false);
            EnsureTrackFile(artist, 1, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_have_artist()
        {
            EnsureProfileCutoff(1, "Lossless");
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", true);
            EnsureTrackFile(artist, 1, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Artist.Should().NotBeNull();
            result.Records.First().Artist.ArtistName.Should().Be("Alien Ant Farm");
        }

        [Test, Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, "Lossless");
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm", false);
            EnsureTrackFile(artist, 1, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
