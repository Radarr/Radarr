using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class FindByTitleFixture : CoreTest<MovieService>
    {
        private List<Movie> _candidates;

        [SetUp]
        public void Setup()
        {
            _candidates = Builder<Movie>.CreateListOfSize(3)
                                        .TheFirst(1)
                                        .With(x => x.CleanTitle = "batman")
                                        .With(x => x.Year = 2000)
                                        .TheNext(1)
                                        .With(x => x.CleanTitle = "batman")
                                        .With(x => x.Year = 1999)
                                        .TheRest()
                                        .With(x => x.CleanTitle = "darkknight")
                                        .With(x => x.Year = 2008)
                                        .With(x => x.AlternativeTitles = new List<AlternativeTitle>
                                        {
                                            new AlternativeTitle
                                            {
                                                CleanTitle = "batman"
                                            }
                                        })
                                        .Build()
                                        .ToList();
        }

        [Test]
        public void should_find_by_title_year()
        {
            var movie = Subject.FindByTitle(new List<string> { "batman" }, 2000, new List<string>(), _candidates);

            movie.Should().NotBeNull();
            movie.Year.Should().Be(2000);
        }

        [Test]
        public void should_find_candidates_by_alt_titles()
        {
            var movie = Subject.FindByTitle(new List<string> { "batman" }, 2008, new List<string>(), _candidates);
            movie.Should().NotBeNull();
            movie.Year.Should().Be(2008);
        }
    }
}
