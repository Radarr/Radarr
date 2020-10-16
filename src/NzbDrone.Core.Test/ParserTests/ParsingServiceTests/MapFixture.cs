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
                                   .With(m => m.CleanTitle = "fackjugoethe2")
                                   .With(m => m.Year = 2015)
                                   .With(m => m.AlternativeTitles = new List<AlternativeTitle> { new AlternativeTitle("Fack Ju Göthe 2: Same same") })
                                   .With(m => m.Translations = new List<MovieTranslation> { new MovieTranslation { Title = "Translated Title", CleanTitle = "translatedtitle" } })
                                   .With(m => m.OriginalLanguage = Language.English)
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
                MovieTitles = new List<string> { _movie.AlternativeTitles.First().Title },
                Languages = new List<Language> { Language.English },
                Year = _movie.Year,
            };

            _translationTitleInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { _movie.Translations.First().Title },
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

        [Test]
        public void should_lookup_Movie_by_name()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", null);

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.FindByTitle(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<List<string>>(), null), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_movie_title()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", _movieSearchCriteria);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_match_with_wrong_year()
        {
            GivenMatchByMovieTitle();
            Subject.Map(_wrongYearInfo, "", _movieSearchCriteria).MappingResultType.Should().Be(MappingResultType.WrongYear);
        }

        [Test]
        public void should_not_match_wrong_title()
        {
            GivenMatchByMovieTitle();
            Subject.Map(_wrongTitleInfo, "", _movieSearchCriteria).MappingResultType.Should().Be(MappingResultType.WrongTitle);
        }

        [Test]
        public void should_return_title_not_found_when_all_is_null()
        {
            Mocker.GetMock<IMovieService>()
                .Setup(s => s.FindByTitle(It.IsAny<string>()))
                .Returns((Movie)null);
            Subject.Map(_parsedMovieInfo, "", null).MappingResultType.Should()
                .Be(MappingResultType.TitleNotFound);
        }

        [Test]
        public void should_match_alternative_title()
        {
            Subject.Map(_alternativeTitleInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_translation_title()
        {
            Subject.Map(_translationTitleInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_roman_title()
        {
            Subject.Map(_romanTitleInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_match_umlauts()
        {
            Subject.Map(_umlautInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
            Subject.Map(_umlautAltInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
        }

        [Test]
        public void should_convert_original()
        {
            Subject.Map(_multiLanguageInfo, "", _movieSearchCriteria).RemoteMovie.ParsedMovieInfo.Languages.Should().Contain(Language.English);
            Subject.Map(_multiLanguageInfo, "", _movieSearchCriteria).RemoteMovie.ParsedMovieInfo.Languages.Should().Contain(Language.French);
        }

        [Test]
        public void should_remove_original_as_already_exists()
        {
            Subject.Map(_multiLanguageWithOriginalInfo, "", _movieSearchCriteria).RemoteMovie.ParsedMovieInfo.Languages.Should().Contain(Language.English);
            Subject.Map(_multiLanguageWithOriginalInfo, "", _movieSearchCriteria).RemoteMovie.ParsedMovieInfo.Languages.Should().Contain(Language.French);
            Subject.Map(_multiLanguageWithOriginalInfo, "", _movieSearchCriteria).RemoteMovie.ParsedMovieInfo.Languages.Should().NotContain(Language.Original);
        }
    }
}
