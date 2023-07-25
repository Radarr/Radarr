using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class GetMoviePathFixture : CoreTest<XbmcService>
    {
        private const string IMDB_ID = "tt67890";
        private XbmcSettings _settings;
        private Movie _movie;
        private List<XbmcMovie> _xbmcMovies;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcMovies = Builder<XbmcMovie>.CreateListOfSize(3)
                                            .All()
                                            .With(s => s.ImdbNumber = "tt00000")
                                            .TheFirst(1)
                                            .With(s => s.ImdbNumber = IMDB_ID)
                                            .Build()
                                            .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetMovies(_settings))
                  .Returns(_xbmcMovies);
        }

        private void GivenMatchingImdbId()
        {
            _movie = new Movie
            {
                ImdbId = IMDB_ID,
                Title = "Movie"
            };
        }

        private void GivenMatchingTitle()
        {
            _movie = new Movie
            {
                ImdbId = "tt01000",
                Title = _xbmcMovies.First().Label
            };
        }

        private void GivenMatchingMovie()
        {
            _movie = new Movie
            {
                ImdbId = "tt01000",
                Title = "Does not exist"
            };
        }

        [Test]
        public void should_return_null_when_movie_is_not_found()
        {
            GivenMatchingMovie();

            Subject.GetMoviePath(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_path_when_tvdbId_matches()
        {
            GivenMatchingImdbId();

            Subject.GetMoviePath(_settings, _movie).Should().Be(_xbmcMovies.First().File);
        }
    }
}
