using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class MovieSpecificationFixture : TestBase<MovieSpecification>
    {
        private Movie _movie1;
        private Movie _movie2;
        private RemoteMovie _remoteEpisode = new RemoteMovie();
        private SearchCriteriaBase _searchCriteria = new MovieSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _movie1 = Builder<Movie>.CreateNew().With(s => s.Id = 1).Build();
            _movie2 = Builder<Movie>.CreateNew().With(s => s.Id = 2).Build();

            _remoteEpisode.Movie = _movie1;
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Movie = _movie2;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Movie = _movie1;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Should().OnlyContain(x => x.Accepted);
        }
    }
}
