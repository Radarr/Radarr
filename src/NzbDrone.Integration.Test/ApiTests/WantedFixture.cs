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
            EnsureNoArtist("266189", "The Blacklist");

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureArtist("266189", "The Blacklist", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_have_artist()
        {
            EnsureArtist("266189", "The Blacklist", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Series.Should().NotBeNull();
            result.Records.First().Series.Title.Should().Be("The Blacklist");
        }

        [Test, Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, Quality.MP3_256);
            var artist = EnsureArtist("266189", "The Blacklist", true);
            EnsureTrackFile(artist, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureArtist("266189", "The Blacklist", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.MP3_256);
            var artist = EnsureArtist("266189", "The Blacklist", false);
            EnsureTrackFile(artist, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_have_artist()
        {
            EnsureProfileCutoff(1, Quality.MP3_256);
            var artist = EnsureArtist("266189", "The Blacklist", true);
            EnsureTrackFile(artist, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Series.Should().NotBeNull();
            result.Records.First().Series.Title.Should().Be("The Blacklist");
        }

        [Test, Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureArtist("266189", "The Blacklist", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.MP3_256);
            var artist = EnsureArtist("266189", "The Blacklist", false);
            EnsureTrackFile(artist, 1, 1, Quality.MP3_192);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
