using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    public class CutoffUnmetFixture : IntegrationTest
    {
        [Test, Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var movie = EnsureMovie(680, "Pulp Fiction", true);
            EnsureMovieFile(movie, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "physicalRelease", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var movie = EnsureMovie(680, "Pulp Fiction", false);
            EnsureMovieFile(movie, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "physicalRelease", "desc", "monitored", "false");

            result.Records.Should().BeEmpty();
        }

        [Test, Order(1)]
        public void cutoff_should_have_movie()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var movie = EnsureMovie(680, "Pulp Fiction", true);
            EnsureMovieFile(movie, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "physicalRelease", "desc");

            result.Records.First().Title.Should().Be("Pulp Fiction");
        }

        [Test, Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var movie = EnsureMovie(680, "Pulp Fiction", false);
            EnsureMovieFile(movie, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "physicalRelease", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
