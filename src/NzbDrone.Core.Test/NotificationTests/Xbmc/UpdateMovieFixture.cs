using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class UpdateMovieFixture : CoreTest<XbmcService>
    {
        private const string IMDB_ID = "tt67890";
        private XbmcSettings _settings;
        private List<XbmcMovie> _xbmcMovies;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcMovies = Builder<XbmcMovie>.CreateListOfSize(3)
                                            .TheFirst(1)
                                            .With(s => s.ImdbNumber = IMDB_ID)
                                            .With(s => s.File = "/movies/Movie (2025)/Movie.mkv")
                                            .Build()
                                            .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetJsonVersion(_settings))
                  .Returns(new Version(13, 5, 0));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetMovies(_settings, true))
                  .Returns(_xbmcMovies);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetActivePlayers(_settings))
                  .Returns(new List<ActivePlayer>());
        }

        [Test]
        public void should_update_using_movie_path()
        {
            var movie = Builder<Movie>.CreateNew()
                                      .With(s => s.ImdbId = IMDB_ID)
                                      .With(s => s.Path = "/data/movies/Movie (2025)")
                                      .Build();

            Subject.Update(_settings, movie);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, "/movies/Movie (2025)/"), Times.Once());
        }

        [Test]
        public void should_update_all_paths_when_movie_path_not_found()
        {
            var fakeMovie = Builder<Movie>.CreateNew()
                                          .With(s => s.ImdbId = "tt01000")
                                          .With(s => s.Title = "Not A Real Movie")
                                          .Build();

            Subject.Update(_settings, fakeMovie);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, null), Times.Once());
        }

        [TestCase("/movies/Movie (2025)/Movie.mkv", "/movies/Movie (2025)/")]
        [TestCase(@"C:\Movies\Movie (2025)\Movie.mkv", @"C:\Movies\Movie (2025)\")]
        [TestCase("smb://nas/movies/Movie (2025)/Movie.mkv", "smb://nas/movies/Movie (2025)/")]
        [TestCase("stack:///movies/Movie (2025)/Movie-cd1.mkv , /movies/Movie (2025)/Movie-cd2.mkv", "/movies/Movie (2025)/")]
        [TestCase("/movies/Movie (2025)/BDMV/index.bdmv", "/movies/Movie (2025)/")]
        [TestCase("smb://nas/movies/Movie (2025)/VIDEO_TS/VIDEO_TS.IFO", "smb://nas/movies/Movie (2025)/")]
        [TestCase(@"C:\Movies\Movie (2025)\bdmv\index.bdmv", @"C:\Movies\Movie (2025)\")]
        public void should_update_folder_of_movie(string kodiFile, string expectedFolder)
        {
            GivenKodiMovieFile(kodiFile);

            Subject.Update(_settings, GivenMovie("/data/movies/Movie (2025)"));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, expectedFolder), Times.Once());
        }

        [Test]
        public void should_update_folder_of_stacked_movie_with_commas_in_its_path()
        {
            GivenKodiMovieFile("stack:///movies/Movie, The (2025)/Movie,, The-cd1.mkv , /movies/Movie,, The (2025)/Movie,, The-cd2.mkv");

            Subject.Update(_settings, GivenMovie("/data/movies/Movie, The (2025)"));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, "/movies/Movie, The (2025)/"), Times.Once());
        }

        [TestCase("Movie.mkv")]
        [TestCase("/movies/Old Name (2025)/Movie.mkv")]
        [TestCase("/movies-4k/Movie (2025)/Movie.mkv")]
        public void should_update_all_paths_when_movie_folder_is_unknown(string kodiFile)
        {
            GivenKodiMovieFile(kodiFile);

            Subject.Update(_settings, GivenMovie("/data/movies/Movie (2025)"));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, null), Times.Once());
        }

        [TestCase("rar://%2fmovies%2fMovie%20(2025)%2fMovie.rar/Movie.mkv")]
        [TestCase("zip://%2fmovies%2fMovie%20(2025)%2fMovie.zip/Movie.mkv")]
        [TestCase("archive://%2fmovies%2fMovie%20(2025)%2fMovie.7z/Movie.mkv")]
        public void should_update_all_paths_when_movie_is_inside_an_archive(string kodiFile)
        {
            GivenKodiMovieFile(kodiFile);

            Subject.Update(_settings, GivenMovie("/data/movies/Movie (2025)"));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, null), Times.Once());
        }

        [Test]
        public void should_update_all_paths_when_movie_has_no_path()
        {
            GivenKodiMovieFile("/movies/Movie (2025)/Movie.mkv");

            Subject.Update(_settings, GivenMovie(null));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, null), Times.Once());
        }

        private static Movie GivenMovie(string path)
        {
            return Builder<Movie>.CreateNew()
                                 .With(s => s.ImdbId = IMDB_ID)
                                 .With(s => s.Path = path)
                                 .Build();
        }

        private void GivenKodiMovieFile(string kodiFile)
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetMovies(_settings, true))
                  .Returns(new List<XbmcMovie> { new XbmcMovie { ImdbNumber = IMDB_ID, File = kodiFile } });
        }
    }
}
