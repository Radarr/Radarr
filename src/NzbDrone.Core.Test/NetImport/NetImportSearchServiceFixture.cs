using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NetImport
{
    [TestFixture]
    public class NetImportSearchServiceFixture : CoreTest<NetImportSearchService>
    {
        private NetImportFetchResult _netImport1Fetch;
        private NetImportFetchResult _netImport2Fetch;
        private List<Movie> _moviesList1;
        private List<Movie> _moviesList2;
        private List<INetImport> _netImports;
        private NetImportSyncCommand _command;

        [SetUp]
        public void Setup()
        {
            _netImports = new List<INetImport>();

            _moviesList1 = Builder<Movie>.CreateListOfSize(5)
                .Build().ToList();

            _moviesList2 = Builder<Movie>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.TmdbId = 6)
                .With(s => s.ImdbId = "6")
                .TheNext(1)
                .With(s => s.TmdbId = 7)
                .With(s => s.ImdbId = "7")
                .TheNext(1)
                .With(s => s.TmdbId = 8)
                .With(s => s.ImdbId = "8")
                .Build().ToList();

            _netImport1Fetch = new NetImportFetchResult
            {
                Movies = _moviesList1,
                AnyFailure = false
            };

            _netImport2Fetch = new NetImportFetchResult
            {
                Movies = _moviesList2,
                AnyFailure = false
            };

            _command = new NetImportSyncCommand
            {
                ListId = 0
            };

            Mocker.GetMock<INetImportFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_netImports);

            Mocker.GetMock<IImportExclusionsService>()
                  .Setup(v => v.IsMovieExcluded(It.IsAny<int>()))
                  .Returns(false);

            Mocker.GetMock<ISearchForNewMovie>()
                  .Setup(v => v.MapMovieToTmdbMovieAsync(It.IsAny<Movie>()))
                  .ReturnsAsync((Movie movie) => movie);

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.MovieExists(It.IsAny<Movie>()))
                  .Returns(false);
        }

        private void GivenListFailure()
        {
            _netImport1Fetch.AnyFailure = true;
        }

        private void GivenCleanLevel(string cleanLevel)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.ListSyncLevel)
                  .Returns(cleanLevel);
        }

        private Mock<INetImport> GivenList(int i, bool enabledAuto, NetImportFetchResult fetchResult)
        {
            var id = i;

            var mockNetImport = new Mock<INetImport>();
            mockNetImport.SetupGet(s => s.Definition).Returns(new NetImportDefinition { Id = id, EnableAuto = enabledAuto });
            mockNetImport.SetupGet(s => s.Enabled).Returns(true);
            mockNetImport.SetupGet(s => s.EnableAuto).Returns(enabledAuto);
            mockNetImport.Setup(s => s.Fetch()).Returns(fetchResult);

            _netImports.Add(mockNetImport.Object);

            return mockNetImport;
        }

        [Test]
        public void should_not_clean_library_if_config_value_disable()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.GetAllMovies(), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(new List<Movie>(), true), Times.Once());
        }

        [Test]
        public void should_log_only_on_clean_library_if_config_value_logonly()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("logOnly");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.GetAllMovies(), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(new List<Movie>(), true), Times.Once());
        }

        [Test]
        public void should_unmonitor_on_clean_library_if_config_value_keepAndUnmonitor()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("keepAndUnmonitor");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.GetAllMovies(), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.Is<List<Movie>>(s => s.Count == 3 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_tmdb_match()
        {
            _netImport1Fetch.Movies[0].TmdbId = 6;

            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("keepAndUnmonitor");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.Is<List<Movie>>(s => s.Count == 2 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_fallback_to_imdbid_on_clean_library_if_tmdb_not_found()
        {
            _netImport1Fetch.Movies[0].TmdbId = 0;
            _netImport1Fetch.Movies[0].ImdbId = "6";

            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("keepAndUnmonitor");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.Is<List<Movie>>(s => s.Count == 2 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_delete_movies_not_files_on_clean_library_if_config_value_logonly()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("removeAndKeep");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.GetAllMovies(), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), false, It.IsAny<bool>()), Times.Exactly(3));

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), true, It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(new List<Movie>(), true), Times.Once());
        }

        [Test]
        public void should_delete_movies_and_files_on_clean_library_if_config_value_logonly()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("removeAndDelete");

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(_moviesList2);

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.GetAllMovies(), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), false, It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.DeleteMovie(It.IsAny<int>(), true, It.IsAny<bool>()), Times.Exactly(3));

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(new List<Movie>(), true), Times.Once());
        }

        [Test]
        public void should_not_clean_if_list_failures()
        {
            GivenListFailure();
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(new List<Movie>(), true), Times.Never());
        }

        [Test]
        public void should_add_new_movies_from_single_list_to_library()
        {
            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 5)), Times.Once());
        }

        [Test]
        public void should_add_new_movies_from_multiple_list_to_library()
        {
            GivenList(1, true, _netImport1Fetch);
            GivenList(2, true, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 8)), Times.Once());
        }

        [Test]
        public void should_add_new_movies_from_enabled_lists_to_library()
        {
            GivenList(1, true, _netImport1Fetch);
            GivenList(2, false, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 5)), Times.Once());
        }

        [Test]
        public void should_not_add_duplicate_movies_from_seperate_lists()
        {
            _netImport2Fetch.Movies[0].TmdbId = 4;

            GivenList(1, true, _netImport1Fetch);
            GivenList(2, true, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 7)), Times.Once());
        }

        [Test]
        public void should_not_add_movie_from_on_exclusion_list()
        {
            GivenList(1, true, _netImport1Fetch);
            GivenList(2, true, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Mocker.GetMock<IImportExclusionsService>()
                  .Setup(v => v.IsMovieExcluded(_moviesList2[0].TmdbId))
                  .Returns(true);

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 7 && !s.Any(m => m.TmdbId == _moviesList2[0].TmdbId))), Times.Once());
        }

        [Test]
        public void should_not_add_movie_that_exists_in_library()
        {
            GivenList(1, true, _netImport1Fetch);
            GivenList(2, true, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Mocker.GetMock<IMovieService>()
                 .Setup(v => v.MovieExists(_moviesList2[0]))
                 .Returns(true);

            Subject.Execute(_command);

            Mocker.GetMock<IAddMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 7 && !s.Any(m => m.TmdbId == _moviesList2[0].TmdbId))), Times.Once());
        }

        /*
        [Test]
        public void should_not_tmdb_map_movie_that_has_tmdbid_from_list()
        {
            GivenList(1, true, _netImport1Fetch);
            GivenList(2, true, _netImport2Fetch);

            GivenCleanLevel("disabled");

            Subject.Execute(_command);

            Mocker.GetMock<ISearchForNewMovie>()
                  .Verify(v => v.MapMovieToTmdbMovie(It.IsAny<Movie>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 8)), Times.Once());
        }

        [Test]
        public void should_tmdb_map_movie_that_has_no_tmdbid_from_list()
        {
            _netImport1Fetch.Movies[0].TmdbId = 0;

            GivenList(1, true, _netImport1Fetch);

            GivenCleanLevel("disabled");

            Mocker.GetMock<ISearchForNewMovie>()
                  .Setup(v => v.MapMovieToTmdbMovie(It.IsAny<Movie>()))
                  .Returns(Builder<Movie>.CreateNew().Build());

            Subject.Execute(_command);

            Mocker.GetMock<ISearchForNewMovie>()
                  .Verify(v => v.MapMovieToTmdbMovie(It.IsAny<Movie>()), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.AddMovies(It.Is<List<Movie>>(s => s.Count == 5)), Times.Once());
        }*/
    }
}
