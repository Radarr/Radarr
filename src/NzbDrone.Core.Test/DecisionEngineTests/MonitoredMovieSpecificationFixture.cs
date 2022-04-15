using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
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
        private Movie _fakeSeries;
        private Movie _firstEpisode;
        private Movie _secondEpisode;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredMovieSpecification>();

            _fakeSeries = Builder<Movie>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstEpisode = new Movie() { Monitored = true };
            _secondEpisode = new Movie() { Monitored = true };

            var singleEpisodeList = new List<Movie> { _firstEpisode };
            var doubleEpisodeList = new List<Movie> { _firstEpisode, _secondEpisode };

            _parseResultMulti = new RemoteMovie
            {
                Movie = _fakeSeries
            };

            _parseResultSingle = new RemoteMovie
            {
                Movie = _fakeSeries
            };
        }

        private void WithMovieUnmonitored()
        {
            _fakeSeries.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Should().OnlyContain(x => x.Accepted);
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithMovieUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new MovieSearchCriteria { UserInvokedSearch = true }).Should().OnlyContain(x => x.Accepted);
        }
    }
}
