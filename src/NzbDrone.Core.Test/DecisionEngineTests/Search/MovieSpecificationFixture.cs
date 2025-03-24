using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
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
        private RemoteMovie _remoteMovie = new();
        private SearchCriteriaBase _searchCriteria = new MovieSearchCriteria();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _movie1 = Builder<Movie>.CreateNew().With(s => s.Id = 1).Build();
            _movie2 = Builder<Movie>.CreateNew().With(s => s.Id = 2).Build();

            _remoteMovie.Movie = _movie1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Movie = _movie2;

            Subject.IsSatisfiedBy(_remoteMovie, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Movie = _movie1;

            Subject.IsSatisfiedBy(_remoteMovie, _information).Accepted.Should().BeTrue();
        }
    }
}
