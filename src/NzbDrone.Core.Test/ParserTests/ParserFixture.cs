using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class ParserFixture : CoreTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         * [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
         * [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
         * [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 }, 2)]
         * (Game of Thrones s03 e - "Game of Thrones Season 3 Episode 10"
         * The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-06.34.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Constantine S1-E1-WEB-DL-1080p-NZBgeek
         */

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Men - Season 1 [Bluray720p]", "madmen")]
        [TestCase("Mad Men - Season 1 [Bluray1080p]", "madmen")]
        [TestCase("The Daily Show With Jon Stewart -", "thedailyshowwithjonstewart")]
        [TestCase("The Venture Bros. (2004)", "theventurebros2004")]
        [TestCase("Castle (2011)", "castle2011")]
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "adventuretime")]
        [TestCase("Hawaii Five 0", "hawaiifive0")]
        [TestCase("Match of the Day", "matchday")]
        [TestCase("Match of the Day 2", "matchday2")]
        [TestCase("[ www.Torrenting.com ] - Revenge.S03E14.720p.HDTV.X264-DIMENSION", "Revenge")]
        [TestCase("Seed S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "Seed")]
        [TestCase("Reno.911.S01.DVDRip.DD2.0.x264-DEEP", "Reno 911")]
        public void should_parse_series_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle).CleanSeriesTitle();
            result.Should().Be(title.CleanSeriesTitle());
        }

        [Test]
        public void should_remove_accents_from_title()
        {
            const string title = "Carniv\u00E0le";
            
            title.CleanSeriesTitle().Should().Be("carnivale");
        }

        [TestCase("Discovery TV - Gold Rush : 02 Road From Hell [S04].mp4")]
        public void should_clean_up_invalid_path_characters(string postTitle)
        {
            Parser.Parser.ParseTitle(postTitle);
        }

        [TestCase("[scnzbefnet][509103] 2.Broke.Girls.S03E18.720p.HDTV.X264-DIMENSION", "2 Broke Girls")]
        public void should_remove_request_info_from_title(string postTitle, string title)
        {
            Parser.Parser.ParseTitle(postTitle).SeriesTitle.Should().Be(title);
        }

		[TestCase("The.Man.from.U.N.C.L.E.2015.1080p.BluRay.x264-SPARKS", "The Man from U.N.C.L.E.")]
		[TestCase("1941.1979.EXTENDED.720p.BluRay.X264-AMIABLE", "1941")]
		[TestCase("MY MOVIE (2016) [R][Action, Horror][720p.WEB-DL.AVC.8Bit.6ch.AC3].mkv", "MY MOVIE")]
		[TestCase("R.I.P.D.2013.720p.BluRay.x264-SPARKS", "R.I.P.D.")]
		[TestCase("V.H.S.2.2013.LIMITED.720p.BluRay.x264-GECKOS", "V.H.S. 2")]
		[TestCase("This Is A Movie (1999) [IMDB #] <Genre, Genre, Genre> {ACTORS} !DIRECTOR +MORE_SILLY_STUFF_NO_ONE_NEEDS ?", "This Is A Movie")]
		[TestCase("We Are the Best!.2013.720p.H264.mkv", "We Are the Best!")]
		[TestCase("(500).Days.Of.Summer.(2009).DTS.1080p.BluRay.x264.NLsubs", "(500) Days Of Summer")]
		public void should_parse_movie_title(string postTitle, string title)
		{
			Parser.Parser.ParseMovieTitle(postTitle).MovieTitle.Should().Be(title);
		}

		[TestCase("1941.1979.EXTENDED.720p.BluRay.X264-AMIABLE", 1979)]
        public void should_parse_movie_year(string postTitle, int year)
		{
			Parser.Parser.ParseMovieTitle(postTitle).Year.Should().Be(year);
		}

		[TestCase("The Danish Girl 2015")]
		public void should_not_parse_language_in_movie_title(string postTitle)
		{
			Parser.Parser.ParseMovieTitle(postTitle).Language.Should().Be(Language.English);
		}
    }
}
