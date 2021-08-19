using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class SearchMovieComparerFixture : CoreTest
    {
        private List<Movie> _series;

        [SetUp]
        public void Setup()
        {
            _series = new List<Movie>();
        }

        private void WithSeries(string title)
        {
            _series.Add(new Movie { Title = title });
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_the_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchMovieComparer("the walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchMovieComparer("walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_blocklist_over_the_blocklist_when_searching_for_blocklist()
        {
            WithSeries("The Blocklist");
            WithSeries("Blocklist");

            _series.Sort(new SearchMovieComparer("blocklist"));

            _series.First().Title.Should().Be("Blocklist");
        }

        [Test]
        public void should_prefer_the_blocklist_over_blocklist_when_searching_for_the_blocklist()
        {
            WithSeries("Blocklist");
            WithSeries("The Blocklist");

            _series.Sort(new SearchMovieComparer("the blocklist"));

            _series.First().Title.Should().Be("The Blocklist");
        }
    }
}
