using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
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

            title.CleanMovieTitle().Should().Be("carnivale");
        }

        [TestCase("The.Movie.from.U.N.C.L.E.2015.1080p.BluRay.x264-SPARKS", "The Movie from U.N.C.L.E.")]
        [TestCase("1776.1979.EXTENDED.720p.BluRay.X264-AMIABLE", "1776")]
        [TestCase("MY MOVIE (2016) [R][Action, Horror][720p.WEB-DL.AVC.8Bit.6ch.AC3].mkv", "MY MOVIE")]
        [TestCase("R.I.P.D.2013.720p.BluRay.x264-SPARKS", "R.I.P.D.")]
        [TestCase("V.H.S.2.2013.LIMITED.720p.BluRay.x264-GECKOS", "V.H.S. 2")]
        [TestCase("This Is A Movie (1999) [IMDB #] <Genre, Genre, Genre> {ACTORS} !DIRECTOR +MORE_SILLY_STUFF_NO_ONE_NEEDS ?", "This Is A Movie")]
        [TestCase("We Are the Movie!.2013.720p.H264.mkv", "We Are the Movie!")]
        [TestCase("(500).Days.Of.Movie.(2009).DTS.1080p.BluRay.x264.NLsubs", "(500) Days Of Movie")]
        [TestCase("To.Live.and.Movie.in.L.A.1985.1080p.BluRay", "To Live and Movie in L.A.")]
        [TestCase("A.I.Artificial.Movie.(2001)", "A.I. Artificial Movie")]
        [TestCase("A.Movie.Name.(1998)", "A Movie Name")]
        [TestCase("www.Torrenting.com - Movie.2008.720p.X264-DIMENSION", "Movie")]
        [TestCase("Movie: The Movie World 2013", "Movie The Movie World")]
        [TestCase("Movie.The.Final.Chapter.2016", "Movie The Final Chapter")]
        [TestCase("Der.Movie.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", "Der Movie James")]
        [TestCase("Movie.German.DL.AC3.Dubbed..BluRay.x264-PsO", "Movie")]
        [TestCase("Valana la Movie FRENCH BluRay 720p 2016 kjhlj", "Valana la Movie")]
        [TestCase("Valana la Movie TRUEFRENCH BluRay 720p 2016 kjhlj", "Valana la Movie")]
        [TestCase("Mission Movie: Rogue Movie (2015)�[XviD - Ita Ac3 - SoftSub Ita]azione, spionaggio, thriller *Prima Visione* Team mulnic Tom Cruise", "Mission Movie Rogue Movie")]
        [TestCase("Movie.Movie.2000.FRENCH..BluRay.-AiRLiNE", "Movie Movie")]
        [TestCase("My Movie 1999 German Bluray", "My Movie")]
        [TestCase("Leaving Movie by Movie (1897) [DVD].mp4", "Leaving Movie by Movie")]
        [TestCase("Movie.2018.1080p.AMZN.WEB-DL.DD5.1.H.264-NTG", "Movie")]
        [TestCase("Movie.Title.Imax.2018.1080p.AMZN.WEB-DL.DD5.1.H.264-NTG", "Movie Title")]
        [TestCase("World.Movie.Z.EXTENDED.2013.German.DL.1080p.BluRay.AVC-XANOR", "World Movie Z")]
        [TestCase("World.Movie.Z.2.EXTENDED.2013.German.DL.1080p.BluRay.AVC-XANOR", "World Movie Z 2")]
        [TestCase("G.I.Movie.Movie.2013.THEATRiCAL.COMPLETE.BLURAY-GLiMMER", "G.I. Movie Movie")]
        [TestCase("www.Torrenting.org - Movie.2008.720p.X264-DIMENSION", "Movie")]
        public void should_parse_movie_title(string postTitle, string title)
        {
            Parser.Parser.ParseMovieTitle(postTitle).MovieTitle.Should().Be(title);
        }

        [TestCase("Movie.Aufbruch.nach.Pandora.Extended.2009.German.DTS.720p.BluRay.x264-SoW", "Movie Aufbruch nach Pandora", "Extended", 2009)]
        [TestCase("Drop.Movie.1994.German.AC3D.DL.720p.BluRay.x264-KLASSiGERHD", "Drop Movie", "", 1994)]
        [TestCase("Kick.Movie.2.2013.German.DTS.DL.720p.BluRay.x264-Pate", "Kick Movie 2", "", 2013)]
        [TestCase("Movie.Hills.2019.German.DL.AC3.Dubbed.1080p.BluRay.x264-muhHD", "Movie Hills", "", 2019)]
        [TestCase("96.Hours.Movie.3.EXTENDED.2014.German.DL.1080p.BluRay.x264-ENCOUNTERS", "96 Hours Movie 3", "EXTENDED", 2014)]
        [TestCase("Movie.War.Q.EXTENDED.CUT.2013.German.DL.1080p.BluRay.x264-HQX", "Movie War Q", "EXTENDED CUT", 2013)]
        [TestCase("Sin.Movie.2005.RECUT.EXTENDED.German.DL.1080p.BluRay.x264-DETAiLS", "Sin Movie", "RECUT EXTENDED", 2005)]
        [TestCase("2.Movie.in.L.A.1996.GERMAN.DL.720p.WEB.H264-SOV", "2 Movie in L.A.", "", 1996)]
        [TestCase("8.2019.GERMAN.720p.BluRay.x264-UNiVERSUM", "8", "", 2019)]
        [TestCase("Life.Movie.2014.German.DL.PAL.DVDR-ETM", "Life Movie", "", 2014)]
        [TestCase("Joe.Movie.2.EXTENDED.EDITION.2015.German.DL.PAL.DVDR-ETM", "Joe Movie 2", "EXTENDED EDITION", 2015)]
        [TestCase("Movie.EXTENDED.2011.HDRip.AC3.German.XviD-POE", "Movie", "EXTENDED", 2011)]

        //Special cases (see description)
        [TestCase("Movie.Klasse.von.1999.1990.German.720p.HDTV.x264-NORETAiL", "Movie Klasse von 1999", "", 1990, Description = "year in the title")]
        [TestCase("Movie.Squad.2016.EXTENDED.German.DL.AC3.BDRip.x264-hqc", "Movie Squad", "EXTENDED", 2016, Description = "edition after year")]
        [TestCase("Movie.and.Movie.2010.Extended.Cut.German.DTS.DL.720p.BluRay.x264-HDS", "Movie and Movie", "Extended Cut", 2010, Description = "edition after year")]
        [TestCase("Der.Movie.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", "Der Movie James", "", 1998, Description = "year at the end")]
        [TestCase("Der.Movie.Eine.Unerwartete.Reise.Extended.German.720p.BluRay.x264-EXQUiSiTE", "Der Movie Eine Unerwartete Reise", "Extended", 0, Description = "no year & edition")]
        [TestCase("Movie.Weg.des.Kriegers.EXTENDED.German.720p.BluRay.x264-EXQUiSiTE", "Movie Weg des Kriegers", "EXTENDED", 0, Description = "no year & edition")]
        [TestCase("Die.Unfassbaren.Movie.Name.EXTENDED.German.DTS.720p.BluRay.x264-RHD", "Die Unfassbaren Movie Name", "EXTENDED", 0, Description = "no year & edition")]
        [TestCase("Die Unfassbaren Movie Name EXTENDED German DTS 720p BluRay x264-RHD", "Die Unfassbaren Movie Name", "EXTENDED", 0, Description = "no year & edition & without dots")]
        [TestCase("Passengers.German.DL.AC3.Dubbed..BluRay.x264-PsO", "Passengers", "", 0, Description = "no year")]
        [TestCase("Das.A.Team.Der.Film.Extended.Cut.German.720p.BluRay.x264-ANCIENT", "Das A Team Der Film", "Extended Cut", 0, Description = "no year")]
        [TestCase("Cars.2.German.DL.720p.BluRay.x264-EmpireHD", "Cars 2", "", 0, Description = "no year")]
        [TestCase("Die.fantastische.Reise.des.Dr.Dolittle.2020.German.DL.LD.1080p.WEBRip.x264-PRD", "Die fantastische Reise des Dr. Dolittle", "", 2020, Description = "dot after dr")]
        [TestCase("Der.Film.deines.Lebens.German.2011.PAL.DVDR-ETM", "Der Film deines Lebens", "", 2011, Description = "year at wrong position")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate_", "Kick Ass 2", "", 2013, Description = "underscore at the end")]
        public void should_parse_german_movie(string postTitle, string title, string edition, int year)
        {
            ParsedMovieInfo movie = Parser.Parser.ParseMovieTitle(postTitle);
            using (new AssertionScope())
            {
                movie.MovieTitle.Should().Be(title);
                movie.Edition.Should().Be(edition);
                movie.Year.Should().Be(year);
            }
        }

        [TestCase("(1995) Movie Name", "Movie Name")]
        public void should_parse_movie_folder_name(string postTitle, string title)
        {
            Parser.Parser.ParseMovieTitle(postTitle, true).MovieTitle.Should().Be(title);
        }

        [TestCase("1776.1979.EXTENDED.720p.BluRay.X264-AMIABLE", 1979)]
        [TestCase("Movie Name FRENCH BluRay 720p 2016 kjhlj", 2016)]
        [TestCase("Der.Movie.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", 1998)]
        [TestCase("Movie Name (1897) [DVD].mp4", 1897)]
        public void should_parse_movie_year(string postTitle, int year)
        {
            Parser.Parser.ParseMovieTitle(postTitle).Year.Should().Be(year);
        }

        [TestCase("Movie Name (2016) {tmdbid-43074}", 43074)]
        [TestCase("Movie Name (2016) [tmdb-43074]", 43074)]
        [TestCase("Movie Name (2016) {tmdb-43074}", 43074)]
        [TestCase("Movie Name (2016) {tmdb-2020}", 2020)]
        public void should_parse_tmdb_id(string postTitle, int tmdbId)
        {
            Parser.Parser.ParseMovieTitle(postTitle).TmdbId.Should().Be(tmdbId);
        }

        [TestCase("The.Italian.Movie.2025.720p.BluRay.X264-AMIABLE")]
        public void should_not_parse_wrong_language_in_title(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count.Should().Be(1);
            parsed.Languages.First().Should().Be(Language.Unknown);
        }

        [TestCase("The.Movie.Name.2016.German.DTS.DL.720p.BluRay.x264-MULTiPLEX")]
        public void should_not_parse_multi_language_in_releasegroup(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count.Should().Be(1);
            parsed.Languages.First().Should().Be(Language.German);
        }

        [TestCase("The.Movie.Name.2016.German.Multi.DTS.DL.720p.BluRay.x264-MULTiPLEX")]
        public void should_parse_multi_language(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count.Should().Be(2);
            parsed.Languages.Should().Contain(Language.German);
            parsed.Languages.Should().Contain(Language.English, "Added by the multi tag in the release name");
        }

        [TestCase("The Italian Job 2008 [tt1234567] 720p BluRay X264", "tt1234567")]
        [TestCase("The Italian Job 2008 [tt12345678] 720p BluRay X264", "tt12345678")]
        public void should_parse_imdb_in_title(string postTitle, string imdb)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.ImdbId.Should().Be(imdb);
        }

        [TestCase("asfd", null)]
        [TestCase("123", "tt0000123")]
        [TestCase("1234567", "tt1234567")]
        [TestCase("tt1234567", "tt1234567")]
        [TestCase("tt12345678", "tt12345678")]
        [TestCase("12345678", "tt12345678")]
        public void should_normalize_imdbid(string imdbid, string normalized)
        {
            Parser.Parser.NormalizeImdbId(imdbid).Should().BeEquivalentTo(normalized);
        }
    }
}
