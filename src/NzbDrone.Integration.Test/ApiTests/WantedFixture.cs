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
                Path = AuthorRootFolder,
                DefaultMetadataProfileId = 1,
                DefaultQualityProfileId = 1,
                DefaultMonitorOption = MonitorTypes.All
            });
        }

        [Test]
        [Order(0)]
        public void missing_should_be_empty()
        {
            EnsureNoArtist("14586394", "Andrew Hunter Murray");

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_artist()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", true);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Author.Should().NotBeNull();
            result.Records.First().Author.AuthorName.Should().Be("Andrew Hunter Murray");
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", true);
            EnsureBookFile(artist, 1, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);
            EnsureBookFile(artist, 1, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_artist()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", true);
            EnsureBookFile(artist, 1, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc");

            result.Records.First().Author.Should().NotBeNull();
            result.Records.First().Author.AuthorName.Should().Be("Andrew Hunter Murray");
        }

        [Test]
        [Order(1)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);

            var result = WantedMissing.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.AZW3);
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);
            EnsureBookFile(artist, 1, 1, Quality.MOBI);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "releaseDate", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
