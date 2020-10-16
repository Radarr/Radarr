using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetMovieFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Movie";

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                  .Verify(s => s.FindByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_series_title()
        {
            const string title = "30.Movie.2015.720p.hdtv";

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                .Verify(s => s.FindByTitle(Parser.Parser.ParseMovieTitle(title, false).MovieTitles, It.IsAny<int>(), It.IsAny<List<string>>(), null), Times.Once());
        }

        /*[Test]
        public void should_fallback_to_title_without_year_and_year_when_title_lookup_fails()
        {
            const string title = "Movie.2004.S01E01.720p.hdtv";
            var parsedEpisodeInfo = Parser.Parser.ParseMovieTitle(title,false,false);

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                  .Verify(s => s.FindByTitle(parsedEpisodeInfo.MovieTitleInfo.TitleWithoutYear,
                                             parsedEpisodeInfo.MovieTitleInfo.Year), Times.Once());
        }*/
    }
}
