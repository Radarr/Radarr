using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredMovieSpecificationFixture : CoreTest<MonitoredMovieSpecification>
    {
        private MonitoredMovieSpecification _monitoredEpisodeSpecification;

        private RemoteMovie _parseResultMulti;
        private RemoteMovie _parseResultSingle;
        private Movie _fakeMovie;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredMovieSpecification>();

            _fakeMovie = Builder<Movie>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _parseResultMulti = new RemoteMovie
            {
                Movie = _fakeMovie
            };

            _parseResultSingle = new RemoteMovie
            {
                Movie = _fakeMovie
            };
        }

        private void WithMovieUnmonitored()
        {
            _fakeMovie.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeMovie.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithMovieUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeMovie.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new MovieSearchCriteria { UserInvokedSearch = true })).Accepted.Should().BeTrue();
        }
    }
}
