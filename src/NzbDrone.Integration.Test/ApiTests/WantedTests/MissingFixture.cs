using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    public class MissingFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void missing_should_be_empty()
        {
            EnsureNoMovie(680, "Pulp Fiction");

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_monitored_items()
        {
            EnsureMovie(680, "Pulp Fiction", true);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void missing_should_have_series()
        {
            EnsureMovie(680, "Pulp Fiction", true);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.First().Title.Should().Be("Pulp Fiction");
        }

        [Test]
        [Order(1)]
        public void missing_should_not_have_unmonitored_items()
        {
            EnsureMovie(680, "Pulp Fiction", false);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void missing_should_have_unmonitored_items()
        {
            EnsureMovie(680, "Pulp Fiction", false);

            var result = WantedMissing.GetPaged(0, 15, "movieMetadata.year", "desc", "monitored", false);

            result.Records.Should().NotBeEmpty();
        }
    }
}
