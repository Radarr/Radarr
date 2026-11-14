using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class GetMovieFolderFixture : CoreTest<XbmcService>
    {
        private const string IMDB_ID = "tt14205554";
        private const int TMDB_ID = 803796;
        private const string FOLDER = "/movies/Movie (2025)/";
        private XbmcSettings _settings;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _movie = new Movie
            {
                ImdbId = IMDB_ID,
                TmdbId = TMDB_ID,
                Title = "Movie",
                Path = "/data/movies/Movie (2025)"
            };

            GivenJsonVersion(_settings, new Version(13, 5, 0));
        }

        [Test]
        public void should_request_unique_ids_from_kodi_17_and_newer()
        {
            GivenJsonVersion(_settings, new Version(8, 0, 0));
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "803796", ("imdb", IMDB_ID), ("tmdb", "803796")));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetMovies(_settings, true), Times.Once());
        }

        [Test]
        public void should_not_request_unique_ids_from_kodi_16_and_older()
        {
            GivenJsonVersion(_settings, new Version(6, 32, 5));
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetMovies(_settings, false), Times.Once());
        }

        [Test]
        public void should_get_json_version_once_per_kodi_host()
        {
            var oldKodiSettings = Builder<XbmcSettings>.CreateNew()
                                                       .With(s => s.Host = "old-kodi")
                                                       .Build();

            GivenJsonVersion(oldKodiSettings, new Version(6, 32, 5));
            GivenKodiMovies();

            Subject.GetMovieFolder(_settings, _movie);
            Subject.GetMovieFolder(_settings, _movie);
            Subject.GetMovieFolder(oldKodiSettings, _movie);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetJsonVersion(_settings), Times.Once());

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetMovies(_settings, true), Times.Exactly(2));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetMovies(oldKodiSettings, false), Times.Once());
        }

        [Test]
        public void should_get_json_version_again_after_test()
        {
            GivenKodiMovies();

            Subject.GetMovieFolder(_settings, _movie);
            Subject.Test(_settings, "Test");
            Subject.GetMovieFolder(_settings, _movie);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.GetJsonVersion(_settings), Times.Exactly(2));
        }

        [Test]
        public void should_return_null_when_kodi_has_no_movies()
        {
            GivenKodiMovies();

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_null_when_movie_is_not_found()
        {
            GivenKodiMovies(KodiMovie("/movies/Other (2020)/Other.mkv", "tt0000001"));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_when_imdbnumber_matches()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_folder_when_unique_ids_match_and_imdbnumber_holds_tmdb_id()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "803796", ("imdb", IMDB_ID), ("tmdb", "803796")));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_folder_when_only_tmdb_unique_id_matches()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "803796", ("tmdb", "803796")));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_folder_when_movie_has_no_imdb_id_and_tmdb_unique_id_matches()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "803796", ("tmdb", "803796")));

            _movie.ImdbId = null;

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_folder_when_unique_id_type_is_unknown_and_imdbnumber_matches()
        {
            // Kodi 17 stores the IDs of a library upgraded from Kodi 16 with the type "unknown"
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", IMDB_ID, ("unknown", IMDB_ID)));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_folder_when_kodi_only_has_imdb_unique_id()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", IMDB_ID, ("imdb", IMDB_ID)));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_null_when_kodi_only_has_imdb_unique_id_of_another_movie()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "tt0000001", ("imdb", "tt0000001")));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_when_tmdb_unique_id_matches_and_imdb_unique_id_differs()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "803796", ("imdb", "tt0000001"), ("tmdb", "803796")));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_null_when_imdb_unique_id_matches_and_tmdb_unique_id_differs()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "12345", ("imdb", IMDB_ID), ("tmdb", "12345")));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_when_tmdb_unique_id_is_not_a_number_and_imdb_unique_id_matches()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "", ("imdb", IMDB_ID), ("tmdb", "")));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_null_when_tmdb_unique_id_is_not_a_number()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", "", ("tmdb", "")));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_null_when_unique_ids_belong_to_another_movie()
        {
            GivenKodiMovies(KodiMovie("/movies/Other (2020)/Other.mkv", "12345", ("imdb", "tt0000001"), ("tmdb", "12345")));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_null_when_movie_has_no_imdb_id_and_kodi_movie_has_no_ids()
        {
            GivenKodiMovies(new XbmcMovie { File = FOLDER + "Other.mkv" });

            _movie.ImdbId = null;

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_of_this_root_when_another_root_has_the_same_movie()
        {
            GivenKodiMovies(KodiMovie("/movies-4k/Movie (2025)/Movie.2160p.mkv", IMDB_ID),
                            KodiMovie(FOLDER + "Movie.1080p.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_null_when_movie_moved_to_another_root()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.mkv", IMDB_ID));

            _movie.Path = "/data/movies-4k/Movie (2025)";

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_when_kodi_has_old_and_new_file_in_the_same_folder()
        {
            GivenKodiMovies(KodiMovie(FOLDER + "Movie.720p.mkv", IMDB_ID),
                            KodiMovie(FOLDER + "Movie.1080p.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().Be(FOLDER);
        }

        [Test]
        public void should_return_null_when_copies_are_in_folders_that_both_match()
        {
            GivenKodiMovies(KodiMovie("smb://nas1/movies/Movie (2025)/Movie.mkv", IMDB_ID),
                            KodiMovie("smb://nas2/movies/Movie (2025)/Movie.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().BeNull();
        }

        [Test]
        public void should_return_folder_when_radarr_uses_windows_path()
        {
            GivenKodiMovies(KodiMovie("smb://nas/movies/Movie (2025)/Movie.mkv", IMDB_ID));

            _movie.Path = @"D:\Movies\Movie (2025)";

            Subject.GetMovieFolder(_settings, _movie).Should().Be("smb://nas/movies/Movie (2025)/");
        }

        [Test]
        public void should_compare_folder_names_ignoring_case()
        {
            GivenKodiMovies(KodiMovie("smb://nas/Movies/movie (2025)/Movie.mkv", IMDB_ID));

            Subject.GetMovieFolder(_settings, _movie).Should().Be("smb://nas/Movies/movie (2025)/");
        }

        private static XbmcMovie KodiMovie(string file, string imdbNumber, params (string Key, string Value)[] uniqueIds)
        {
            return new XbmcMovie
            {
                File = file,
                ImdbNumber = imdbNumber,
                UniqueId = uniqueIds.Any() ? uniqueIds.ToDictionary(u => u.Key, u => u.Value) : null
            };
        }

        private void GivenJsonVersion(XbmcSettings settings, Version version)
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetJsonVersion(settings))
                  .Returns(version);
        }

        private void GivenKodiMovies(params XbmcMovie[] movies)
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetMovies(It.IsAny<XbmcSettings>(), It.IsAny<bool>()))
                  .Returns(movies.ToList());
        }
    }
}
