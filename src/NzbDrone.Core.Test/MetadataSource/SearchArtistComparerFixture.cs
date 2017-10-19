using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class SearchArtistComparerFixture : CoreTest
    {
        private List<Artist> _artist;

        [SetUp]
        public void Setup()
        {
            _artist = new List<Artist>();
        }

        private void WithSeries(string name)
        {
            _artist.Add(new Artist { Name = name });
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_the_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _artist.Sort(new SearchArtistComparer("the walking dead"));

            _artist.First().Name.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _artist.Sort(new SearchArtistComparer("walking dead"));

            _artist.First().Name.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_blacklist_over_the_blacklist_when_searching_for_blacklist()
        {
            WithSeries("The Blacklist");
            WithSeries("Blacklist");

            _artist.Sort(new SearchArtistComparer("blacklist"));

            _artist.First().Name.Should().Be("Blacklist");
        }

        [Test]
        public void should_prefer_the_blacklist_over_blacklist_when_searching_for_the_blacklist()
        {
            WithSeries("Blacklist");
            WithSeries("The Blacklist");

            _artist.Sort(new SearchArtistComparer("the blacklist"));

            _artist.First().Name.Should().Be("The Blacklist");
        }
    }
}
