using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class MapFixture : TestBase<ParsingService>
    {
        private Movie _movie;
        private ParsedMovieInfo _parsedMovieInfo;
        private ParsedMovieInfo _wrongYearInfo;
        private ParsedMovieInfo _wrongTitleInfo;
        private ParsedMovieInfo _romanTitleInfo;
        private ParsedMovieInfo _alternativeTitleInfo;
        private ParsedMovieInfo _translationTitleInfo;
        private ParsedMovieInfo _umlautInfo;
        private ParsedMovieInfo _umlautAltInfo;
        private ParsedMovieInfo _multiLanguageInfo;
        private ParsedMovieInfo _multiLanguageWithOriginalInfo;
        private MovieSearchCriteria _movieSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.Title = "Fack Ju Göthe 2")
                                   .With(m => m.MovieMetadata.Value.CleanTitle = "fackjugoethe2")
                                   .With(m => m.Year = 2015)
                                   .With(m => m.MovieMetadata.Value.AlternativeTitles = new List<AlternativeTitle> { new AlternativeTitle("Fack Ju Göthe 2: Same same") })
                                   .With(m => m.MovieMetadata.Value.Translations = new List<MovieTranslation> { new MovieTranslation { Title = "Translated Title", CleanTitle = "translatedtitle" } })
                                   .With(m => m.MovieMetadata.Value.OriginalLanguage = Language.English)
                                   .Build();

            _parsedMovieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { _movie.Title },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year,
            };

            _wrongYearInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { _movie.Title },
                Languages = new List<Language> { Language.English },
                Year = 1900,
            };

            _wrongTitleInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { "Other Title" },
                Languages = new List<Language> { Language.English },
                Year = 2015
            };

            _alternativeTitleInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { _movie.MovieMetadata.Value.AlternativeTitles.First().Title },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year,
            };

            _translationTitleInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { _movie.MovieMetadata.Value.Translations.First().Title },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year,
            };

            _romanTitleInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { "Fack Ju Göthe II" },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year,
            };

            _umlautInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { "Fack Ju Goethe 2" },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year
            };

            _umlautAltInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { "Fack Ju Goethe 2: Same same" },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year
            };

            _multiLanguageInfo = new ParsedMovieInfo
            {
                MovieTitles = { _movie.Title },
                Languages = new List<Language> { Language.Original, Language.French }
            };

            _multiLanguageWithOriginalInfo = new ParsedMovieInfo
            {
                MovieTitles = { _movie.Title },
                Languages = new List<Language> { Language.Original, Language.French, Language.English }
            };

            _movieSearchCriteria = new MovieSearchCriteria
            {
                Movie = _movie
            };
        }

        private void GivenMatchByMovieTitle()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_movie);
        }

        private Movie GivenLibraryMovie(string title, int year, string imdbId, int tmdbId)
        {
            var movie = Builder<Movie>.CreateNew()
                                      .With(m => m.Title = title)
                                      .With(m => m.Year = year)
                                      .With(m => m.ImdbId = imdbId)
                                      .With(m => m.TmdbId = tmdbId)
                                      .With(m => m.MovieMetadata.Value.CleanTitle = title.CleanMovieTitle())
                                      .With(m => m.MovieMetadata.Value.CleanOriginalTitle = title.CleanMovieTitle())
                                      .With(m => m.MovieMetadata.Value.SecondaryYear = null)
                                      .With(m => m.MovieMetadata.Value.AlternativeTitles = new List<AlternativeTitle>())
                                      .With(m => m.MovieMetadata.Value.Translations = new List<MovieTranslation>())
                                      .Build();

            // Same lookup as MovieRepository.FindByImdbId, which normalizes the ID first
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.FindByImdbId(It.Is<string>(i => imdbId != null && Parser.Parser.NormalizeImdbId(i) == imdbId)))
                  .Returns(movie);

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.FindByTmdbId(tmdbId))
                  .Returns(movie);

            return movie;
        }

        [Test]
        public void should_lookup_Movie_by_name()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", 0, null);

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.FindByTitle(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<List<string>>(), null), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_movie_title()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", 0, _movieSearchCriteria);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_match_alternative_title()
        {
            Subject.Map(_alternativeTitleInfo, "", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_translation_title()
        {
            Subject.Map(_translationTitleInfo, "", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_roman_title()
        {
            Subject.Map(_romanTitleInfo, "", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_umlauts()
        {
            Subject.Map(_umlautInfo, "", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
            Subject.Map(_umlautAltInfo, "", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_search_criteria_movie_by_imdb_id_without_prefix()
        {
            _movie.ImdbId = "tt6478412";
            _wrongYearInfo.Year = _movie.Year - 1;

            Subject.Map(_wrongYearInfo, "6478412", 0, _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_not_match_search_criteria_movie_without_imdb_id_when_release_has_none()
        {
            _movie.ImdbId = null;

            Subject.Map(_wrongYearInfo, "0", 0, _movieSearchCriteria).Movie.Should().BeNull();
        }

        [TestCase("Flatliners.1990.BluRay.2160p.UHD.REMUX", "Flatliners", 2017, "tt2039338", 400710, "2039338", 0)]
        [TestCase("The Hills Have Eyes 1977 2160p UHD BluRay REMUX", "The Hills Have Eyes", 2006, "tt0454841", 9792, "454841", 0)]
        [TestCase("Dont Tell Mom the Babysitters Dead 2024 2160p AMZN WEB-DL", "Don't Tell Mom the Babysitter's Dead", 1991, "tt0101757", 15413, "101757", 0)]
        [TestCase("The Person Who Sees Time 2026 2160p WEB-DL", "In Time", 2011, "tt1637688", 49530, "1637688", 0)]
        [TestCase("The.Witch.Next.Door.2019.2160p.UHD.BluRay", "The Girl Next Door", 2004, "tt0265208", 10591, "0", 10591)]
        public void should_not_match_search_criteria_movie_by_id_when_release_is_another_movie(string releaseTitle, string title, int year, string imdbId, int tmdbId, string releaseImdbId, int releaseTmdbId)
        {
            var movie = GivenLibraryMovie(title, year, imdbId, tmdbId);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(releaseTitle);

            Subject.Map(parsedMovieInfo, releaseImdbId, releaseTmdbId, new MovieSearchCriteria { Movie = movie }).Movie.Should().BeNull();
        }

        [Test]
        public void should_not_match_search_criteria_movie_by_id_when_release_is_same_titled_movie_from_another_year()
        {
            var movie = GivenLibraryMovie("Casino Royale", 1967, "tt0061452", 12208);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Casino.Royale.2006.2160p.BluRay");

            Subject.Map(parsedMovieInfo, "61452", 0, new MovieSearchCriteria { Movie = movie }).Movie.Should().BeNull();
        }

        [TestCase("Isao.Takahata.and.His.Tale.of.the.Princess.Kaguya.2015.1080p.AMZN.WEB-DL.DDP2.0.H.264-GROUP")]
        [TestCase("Isao.Takahata.and.His.Tale.of.the.Princess.Kaguya.2013.1080p.AMZN.WEB-DL.DDP2.0.H.264-GROUP")]
        public void should_match_search_criteria_movie_by_imdb_id_when_release_year_is_one_year_off(string releaseTitle)
        {
            var movie = GivenLibraryMovie("Isao Takahata and His Tale of The Princess Kaguya", 2014, "tt6478412", 332406);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(releaseTitle);

            var result = Subject.Map(parsedMovieInfo, "6478412", 0, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [Test]
        public void should_match_search_criteria_movie_by_tmdb_id_when_release_year_is_one_year_off()
        {
            var movie = GivenLibraryMovie("Leprechaun", 1993, "tt0107387", 11811);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Leprechaun.1992.REPACK2.UHD.BluRay.2160p");

            var result = Subject.Map(parsedMovieInfo, "0", 11811, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [TestCase("0", 0, MovieMatchType.Title)]
        [TestCase("2039338", 400710, MovieMatchType.Id)]
        public void should_match_search_criteria_movie_when_release_title_and_year_match(string releaseImdbId, int releaseTmdbId, MovieMatchType matchType)
        {
            var movie = GivenLibraryMovie("Flatliners", 2017, "tt2039338", 400710);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Flatliners.2017.1080p.BluRay.x264-GROUP");

            var result = Subject.Map(parsedMovieInfo, releaseImdbId, releaseTmdbId, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(matchType);
        }

        [Test]
        public void should_match_search_criteria_movie_by_title_when_release_has_no_year()
        {
            var movie = GivenLibraryMovie("5 Centimeters per Second", 2007, null, 0);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("[Group] 5 Centimeters per Second (BDRip 1920x1080 Hi10 FLAC) [FD8B6FF2].mkv");

            parsedMovieInfo.Year.Should().Be(0);
            Subject.Map(parsedMovieInfo, "0", 0, new MovieSearchCriteria { Movie = movie }).Movie.Should().Be(movie);
        }

        [Test]
        public void should_not_match_search_criteria_movie_by_id_when_release_year_is_two_years_off()
        {
            var movie = GivenLibraryMovie("Leprechaun", 1993, "tt0107387", 11811);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Leprechaun.1991.1080p.BluRay.x264-GROUP");

            Subject.Map(parsedMovieInfo, "0", 11811, new MovieSearchCriteria { Movie = movie }).Movie.Should().BeNull();
        }

        [Test]
        public void should_match_search_criteria_movie_by_id_when_release_year_is_one_year_from_secondary_year()
        {
            var movie = GivenLibraryMovie("Flatliners", 2017, "tt2039338", 400710);
            movie.MovieMetadata.Value.SecondaryYear = 2015;
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Flatliners.2014.1080p.BluRay.x264-GROUP");

            var result = Subject.Map(parsedMovieInfo, "2039338", 0, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [Test]
        public void should_not_match_search_criteria_movie_by_id_when_release_year_is_more_than_one_year_from_both_years()
        {
            var movie = GivenLibraryMovie("Flatliners", 2017, "tt2039338", 400710);
            movie.MovieMetadata.Value.SecondaryYear = 2015;
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Flatliners.2013.1080p.BluRay.x264-GROUP");

            Subject.Map(parsedMovieInfo, "2039338", 0, new MovieSearchCriteria { Movie = movie }).Movie.Should().BeNull();
        }

        [Test]
        public void should_match_search_criteria_movie_by_id_when_movie_has_no_year()
        {
            var movie = GivenLibraryMovie("Flatliners", 0, "tt2039338", 400710);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Flatliners.2017.1080p.BluRay.x264-GROUP");

            var result = Subject.Map(parsedMovieInfo, "2039338", 0, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [Test]
        public void should_match_search_criteria_movie_by_id_when_release_title_differs_and_year_is_one_year_off()
        {
            var movie = GivenLibraryMovie("Flatliners", 2017, "tt2039338", 400710);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle("Other.Title.2018.1080p.BluRay.x264-GROUP");

            var result = Subject.Map(parsedMovieInfo, "2039338", 0, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [TestCase("Blade.Runner.2049.1080p.BluRay.x264-GROUP", "Blade Runner 2049", 2017, "12345", 0)]
        [TestCase("Death.Race.2000.1080p.BluRay.x264-GROUP", "Death Race 2000", 1975, "0", 12345)]
        public void should_match_search_criteria_movie_by_id_when_release_year_is_part_of_the_title(string releaseTitle, string title, int year, string releaseImdbId, int releaseTmdbId)
        {
            var movie = GivenLibraryMovie(title, year, "tt0012345", 12345);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(releaseTitle);

            var result = Subject.Map(parsedMovieInfo, releaseImdbId, releaseTmdbId, new MovieSearchCriteria { Movie = movie });

            result.Movie.Should().Be(movie);
            result.MovieMatchType.Should().Be(MovieMatchType.Id);
        }

        [TestCase("Death.Race.2008.1080p.BluRay.x264-GROUP", "Death Race 2000", 1975)]
        public void should_not_match_search_criteria_movie_by_id_when_release_is_another_movie_with_a_year_in_the_title(string releaseTitle, string title, int year)
        {
            var movie = GivenLibraryMovie(title, year, "tt0012345", 12345);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(releaseTitle);

            Subject.Map(parsedMovieInfo, "0", 12345, new MovieSearchCriteria { Movie = movie }).Movie.Should().BeNull();
        }
    }
}
