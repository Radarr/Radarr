using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Qualities;
using Readarr.Api.V1.RootFolders;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class WantedFixture : IntegrationTest
    {
        [SetUp]
        public void Setup()
        {
            // Add a root folder
            RootFolders.Post(new RootFolderResource
            {
                Name = "TestLibrary",
                Path = ArtistRootFolder,
                DefaultMetadataProfileId = 1,
                DefaultQualityProfileId = 1,
                DefaultMonitorOption = MonitorTypes.All
            });
        }

        [Test]
        [Order(0)]
        public void missing_should_be_empty()
        {
            EnsureNoArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "J.K. Rowling");

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_artist()
        {
            EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Artist.Should().NotBeNull();
            result.Records.First().Artist.ArtistName.Should().Be("J.K. Rowling");
        }

        [Test]
        [Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", true);
            EnsureTrackFile(artist, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);
            EnsureTrackFile(artist, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_have_artist()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", true);
            EnsureTrackFile(artist, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Artist.Should().NotBeNull();
            result.Records.First().Artist.ArtistName.Should().Be("J.K. Rowling");
        }

        [Test]
        [Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);
            EnsureTrackFile(artist, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
