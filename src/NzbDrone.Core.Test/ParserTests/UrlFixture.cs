using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class UrlFixture : CoreTest
    {
        [TestCase("[www.test.com] - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("test.net - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("[www.test-hyphen.com] - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("www.test123.org - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("[test.co.uk] - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("www.test-hyphen.net.au - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("[www.test123.co.nz] - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("test-hyphen123.org.au - Movie.Title.2023.720p.HDTV.X264-DIMENSION", "Movie Title")]
        [TestCase("[www.test123.de] - Mad Movie Title 2023 [Bluray720p]", "Mad Movie Title")]
        [TestCase("www.test-hyphen.de - Mad Movie Title 2023 [Bluray1080p]", "Mad Movie Title")]
        [TestCase("www.test123.co.za - The Movie Title Bros. (2023)", "The Movie Title Bros.")]
        [TestCase("[www.test-hyphen.ca] - Movie Title (2023)", "Movie Title")]
        [TestCase("test123.ca - Movie Time 2023 720p HDTV x264 CRON", "Movie Time")]
        [TestCase("[www.test-hyphen123.co.za] - Movie Title 2023", "Movie Title")]
        [TestCase("(movieawake.com) Movie Title 2023 [720p] [English Subbed]", "Movie Title")]
        public void should_not_parse_url_in_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle).MovieTitle.CleanMovieTitle();
            result.Should().Be(title.CleanMovieTitle());
        }

        [TestCase("Movie.2023.English.HDTV.XviD-LOL[www.abb.com]", "LOL")]
        [TestCase("Movie Title 2023 English HDTV XviD LOL[www.academy.org]", null)]
        [TestCase("Movie Title Now 2023 DVDRip XviD RUNNER[www.aetna.net]", null)]
        [TestCase("Movie.Title.2023.DVDRip.XviD-RUNNER[www.alfaromeo.io]", "RUNNER")]
        [TestCase("Movie.Title.2023.English.HDTV.XviD-LOL[www.abbott.gov]", "LOL")]
        [TestCase("Movie Title 2023 English HDTV XviD LOL[www.actor.org]", null)]
        [TestCase("Movie Title Future 2023 DVDRip XviD RUNNER[www.allstate.net]", null)]
        public void should_not_parse_url_in_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
