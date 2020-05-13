using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class SearchArtistComparerFixture : CoreTest
    {
        private List<Author> _artist;

        [SetUp]
        public void Setup()
        {
            _artist = new List<Author>();
        }

        private void WithSeries(string name)
        {
            _artist.Add(new Author { Name = name });
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_the_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _artist.Sort(new SearchAuthorComparer("the walking dead"));

            _artist.First().Name.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _artist.Sort(new SearchAuthorComparer("walking dead"));

            _artist.First().Name.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_blacklist_over_the_blacklist_when_searching_for_blacklist()
        {
            WithSeries("The Blacklist");
            WithSeries("Blacklist");

            _artist.Sort(new SearchAuthorComparer("blacklist"));

            _artist.First().Name.Should().Be("Blacklist");
        }

        [Test]
        public void should_prefer_the_blacklist_over_blacklist_when_searching_for_the_blacklist()
        {
            WithSeries("Blacklist");
            WithSeries("The Blacklist");

            _artist.Sort(new SearchAuthorComparer("the blacklist"));

            _artist.First().Name.Should().Be("The Blacklist");
        }
    }
}
