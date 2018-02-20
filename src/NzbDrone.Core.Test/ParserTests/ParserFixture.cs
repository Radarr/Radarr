using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
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

        [Test]
        public void should_remove_accents_from_title()
        {
            const string title = "Carniv\u00E0le";

            title.CleanSeriesTitle().Should().Be("carnivale");
        }

        //Note: This assumes extended language parser is activated
		[TestCase("The.Man.from.U.N.C.L.E.2015.1080p.BluRay.x264-SPARKS", "The Man from U.N.C.L.E.")]
		[TestCase("1941.1979.EXTENDED.720p.BluRay.X264-AMIABLE", "1941")]
		[TestCase("MY MOVIE (2016) [R][Action, Horror][720p.WEB-DL.AVC.8Bit.6ch.AC3].mkv", "MY MOVIE")]
		[TestCase("R.I.P.D.2013.720p.BluRay.x264-SPARKS", "R.I.P.D.")]
		[TestCase("V.H.S.2.2013.LIMITED.720p.BluRay.x264-GECKOS", "V.H.S. 2")]
		[TestCase("This Is A Movie (1999) [IMDB #] <Genre, Genre, Genre> {ACTORS} !DIRECTOR +MORE_SILLY_STUFF_NO_ONE_NEEDS ?", "This Is A Movie")]
		[TestCase("We Are the Best!.2013.720p.H264.mkv", "We Are the Best!")]
		[TestCase("(500).Days.Of.Summer.(2009).DTS.1080p.BluRay.x264.NLsubs", "(500) Days Of Summer")]
		[TestCase("To.Live.and.Die.in.L.A.1985.1080p.BluRay", "To Live and Die in L.A.")]
		[TestCase("A.I.Artificial.Intelligence.(2001)", "A.I. Artificial Intelligence")]
		[TestCase("A.Movie.Name.(1998)", "A Movie Name")]
		[TestCase("Thor: The Dark World 2013", "Thor The Dark World")]
        [TestCase("Resident.Evil.The.Final.Chapter.2016", "Resident Evil The Final Chapter")]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", "Der Soldat James")]
        [TestCase("Passengers.German.DL.AC3.Dubbed..BluRay.x264-PsO", "Passengers")]
        [TestCase("Valana la Legende FRENCH BluRay 720p 2016 kjhlj", "Valana la Legende")]
        [TestCase("Valana la Legende TRUEFRENCH BluRay 720p 2016 kjhlj", "Valana la Legende")]
        [TestCase("Mission Impossible: Rogue Nation (2015)�[XviD - Ita Ac3 - SoftSub Ita]azione, spionaggio, thriller *Prima Visione* Team mulnic Tom Cruise", "Mission Impossible Rogue Nation")]
		[TestCase("Scary.Movie.2000.FRENCH..BluRay.-AiRLiNE", "Scary Movie")]
		[TestCase("My Movie 1999 German Bluray", "My Movie")]
		public void should_parse_movie_title(string postTitle, string title)
		{
		    Parser.Parser.ParseMovieTitle(postTitle, true).MovieTitle.Should().Be(title);
        }

        [TestCase("(1995) Ghost in the Shell", "Ghost in the Shell")]
        public void should_parse_movie_folder_name(string postTitle, string title)
        {
            Parser.Parser.ParseMovieTitle(postTitle, true, true).MovieTitle.Should().Be(title);
        }

        [TestCase("1941.1979.EXTENDED.720p.BluRay.X264-AMIABLE", 1979)]
        [TestCase("Valana la Legende FRENCH BluRay 720p 2016 kjhlj", 2016)]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", 1998)]
        public void should_parse_movie_year(string postTitle, int year)
		{
			Parser.Parser.ParseMovieTitle(postTitle, false).Year.Should().Be(year);
		}

        [TestCase("Prometheus 2012 Directors Cut", "Directors Cut")]
        [TestCase("Star Wars Episode IV - A New Hope 1999 (Despecialized).mkv", "Despecialized")]
        [TestCase("Prometheus.2012.(Special.Edition.Remastered).[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Prometheus 2012 Extended", "Extended")]
        [TestCase("Prometheus 2012 Extended Directors Cut Fan Edit", "Extended Directors Cut Fan Edit")]
        [TestCase("Prometheus 2012 Director's Cut", "Director's Cut")]
        [TestCase("Prometheus 2012 Directors Cut", "Directors Cut")]
        [TestCase("Prometheus.2012.(Extended.Theatrical.Version.IMAX).BluRay.1080p.2012.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("2001 A Space Odyssey (1968) Director's Cut .mkv", "Director's Cut")]
        [TestCase("2001: A Space Odyssey 1968 (Extended Directors Cut FanEdit)", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 2012 Directors.mkv", "Directors")]
        [TestCase("Blade Runner 2049 Director's Cut.mkv", "Director's Cut")]
        [TestCase("Prometheus 2012 50th Anniversary Edition.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2012 2in1.mkv", "2in1")]
        [TestCase("Movie 2012 IMAX.mkv", "IMAX")]
        [TestCase("Movie 2012 Restored.mkv", "Restored")]
        [TestCase("Prometheus.Special.Edition.Fan Edit.2012..BRRip.x264.AAC-m2g", "Special Edition Fan Edit")]
        [TestCase("Star Wars Episode IV - A New Hope (Despecialized) 1999.mkv", "Despecialized")]
        [TestCase("Prometheus.(Special.Edition.Remastered).2012.[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Prometheus Extended 2012", "Extended")]
        [TestCase("Prometheus Extended Directors Cut Fan Edit 2012", "Extended Directors Cut Fan Edit")]
        [TestCase("Prometheus Director's Cut 2012", "Director's Cut")]
        [TestCase("Prometheus Directors Cut 2012", "Directors Cut")]
        [TestCase("Prometheus.(Extended.Theatrical.Version.IMAX).2012.BluRay.1080p.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("2001 A Space Odyssey Director's Cut (1968).mkv", "Director's Cut")]
        [TestCase("2001: A Space Odyssey (Extended Directors Cut FanEdit) 1968 Bluray 1080p", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 Directors 2012.mkv", "Directors")]
        [TestCase("Blade Runner Director's Cut 2049.mkv", "Director's Cut")]
        [TestCase("Prometheus 50th Anniversary Edition 2012.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2in1 2012.mkv", "2in1")]
        [TestCase("Movie IMAX 2012.mkv", "IMAX")]
        [TestCase("Fake Movie Final Cut 2016", "Final Cut")]
        [TestCase("Fake Movie 2016 Final Cut ", "Final Cut")]
        [TestCase("My Movie GERMAN Extended Cut 2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut.2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut", "Extended Cut")]
        [TestCase("Mission Impossible: Rogue Nation 2012 Bluray", "")]
        public void should_parse_edition(string postTitle, string edition)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            if (parsed.Edition.IsNullOrWhiteSpace())
            {
                parsed.Edition = Parser.Parser.ParseEdition(parsed.SimpleTitle.Replace(parsed.MovieTitle, "A Movie"));
            }
            parsed.Edition.Should().Be(edition);
        }

        [TestCase("The Lord of the Rings The Fellowship of the Ring (Extended Edition) 1080p BD25", "The Lord Of The Rings The Fellowship Of The Ring", "Extended Edition")]
        [TestCase("The.Lord.of.the.Rings.The.Fellowship.of.the.Ring.(Extended.Edition).1080p.BD25", "The Lord Of The Rings The Fellowship Of The Ring", "Extended Edition")]
        public void should_parse_edition_lenient_mapping(string postTitle, string foundTitle, string edition)
        {
            Parser.Parser.ParseMinimalMovieTitle(postTitle, foundTitle, 1290).Edition.Should().Be(edition);
        }
    }
}
