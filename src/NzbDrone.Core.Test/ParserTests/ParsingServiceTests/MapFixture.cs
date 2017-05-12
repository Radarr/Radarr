using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class MapFixture : TestBase<ParsingService>
    {
        private Series _series;
		private Movie _movie;
		private ParsedMovieInfo _parsedMovieInfo;
		private ParsedMovieInfo _wrongYearInfo;
		private ParsedMovieInfo _romanTitleInfo;
		private ParsedMovieInfo _alternativeTitleInfo;
        private ParsedMovieInfo _umlautInfo;
        private ParsedMovieInfo _umlautAltInfo;
		private MovieSearchCriteria _movieSearchCriteria;
        private List<Episode> _episodes;
        private ParsedEpisodeInfo _parsedEpisodeInfo;
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.CleanTitle = "rock")
                .Build();

			_movie = Builder<Movie>.CreateNew()
								   .With(m => m.Title = "Fack Ju Göthe 2")
								   .With(m => m.CleanTitle = "fackjugoethe2")
			                       .With(m => m.Year = 2015)
			                       .With(m => m.AlternativeTitles = new List<string> { "Fack Ju Göthe 2: Same same" })
								   .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                        .Build()
                                        .ToList();

            _parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = _series.Title,
                SeasonNumber = 1,
                EpisodeNumbers = new[] { 1 }
            };

			_parsedMovieInfo = new ParsedMovieInfo
			{
				MovieTitle = _movie.Title,
				Year = _movie.Year,

			};

			_wrongYearInfo = new ParsedMovieInfo
			{
				MovieTitle = _movie.Title,
				Year = 1900,
			};

			_alternativeTitleInfo = new ParsedMovieInfo
			{
				MovieTitle = _movie.AlternativeTitles.First(),
				Year = _movie.Year,
			};

			_romanTitleInfo = new ParsedMovieInfo
			{
				MovieTitle = "Fack Ju Göthe II",
				Year = _movie.Year,
			};

            _umlautInfo = new ParsedMovieInfo
            {
                MovieTitle = "Fack Ju Goethe 2",
                Year = _movie.Year
            };

            _umlautAltInfo = new ParsedMovieInfo
            {
                MovieTitle = "Fack Ju Goethe 2: Same same",
                Year = _movie.Year
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = _series,
                EpisodeNumber = _episodes.First().EpisodeNumber,
                SeasonNumber = _episodes.First().SeasonNumber,
                Episodes = _episodes
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

        private void GivenParseResultSeriesDoesntMatchSearchCriteria()
        {
            _parsedEpisodeInfo.SeriesTitle = "Another Name";
        }

        [Test]
        public void should_lookup_Movie_by_name()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", null);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_movie_title()
        {
            GivenMatchByMovieTitle();

            Subject.Map(_parsedMovieInfo, "", _movieSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_match_with_wrong_year()
		{
			GivenMatchByMovieTitle();
			Subject.Map(_wrongYearInfo, "", _movieSearchCriteria).Movie.Should().BeNull();
        }

		[Test]
		public void should_match_alternative_title()
		{
			Subject.Map(_alternativeTitleInfo, "", _movieSearchCriteria).Movie.Should().Be(_movieSearchCriteria.Movie);
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

    }
}
