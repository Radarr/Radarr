using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class GetMoviePathFixture : CoreTest<XbmcService>
    {
        // Sometimes Kodi will return TMDbID as ImdbNumber
        private const string IMDB_ID = "tt1254207";
        private const int TMDB_ID = 10378;
        private const string BasePath = @"C:\Test\Downloaded";
        private XbmcSettings _settings;
        private Movie _movie;
        private List<XbmcMovie> _xbmcMovies;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcMovies = Builder<XbmcMovie>.CreateListOfSize(4)
                                            .All()
                                            .With(s => s.ImdbNumber = "tt00000")
                                            .TheFirst(1)
                                            .With(s => s.ImdbNumber = IMDB_ID)
                                            .TheLast(1)
                                            .With(s => s.ImdbNumber = TMDB_ID.ToString())
                                            .Build()
                                            .ToList();

            for (var i = 0; i < _xbmcMovies.Count; i++)
            {
                var filePath = System.IO.Path.Combine(BasePath, $"File{i + 1}", $"file{i + 1}.mkv");
                _xbmcMovies[i].File = filePath.AsOsAgnostic();
            }

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

        private void GivenMatchingTmdbId()
        {
            _movie = new Movie
            {
                TmdbId = TMDB_ID,
                Title = "Movie"
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
        public void should_return_path_when_imdbId_matches()
        {
            GivenMatchingImdbId();

            var expectedParentPath = new DirectoryInfo(_xbmcMovies.First().File).Parent.FullName;
            Subject.GetMoviePath(_settings, _movie).Should().Be(expectedParentPath);
        }

        [Test]
        public void should_return_path_when_tmdbId_matches()
        {
            GivenMatchingTmdbId();

            var expectedParentPath = new DirectoryInfo(_xbmcMovies.Last().File).Parent.FullName;
            Subject.GetMoviePath(_settings, _movie).Should().Be(expectedParentPath);
        }
    }
}
