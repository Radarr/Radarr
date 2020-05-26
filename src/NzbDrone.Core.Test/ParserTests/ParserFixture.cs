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

            title.CleanSeriesTitle().Should().Be("carnivale");
        }

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
        [TestCase("www.Torrenting.com - Revenge.2008.720p.X264-DIMENSION", "Revenge")]
        [TestCase("Thor: The Dark World 2013", "Thor The Dark World")]
        [TestCase("Resident.Evil.The.Final.Chapter.2016", "Resident Evil The Final Chapter")]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", "Der Soldat James")]
        [TestCase("Passengers.German.DL.AC3.Dubbed..BluRay.x264-PsO", "Passengers")]
        [TestCase("Valana la Legende FRENCH BluRay 720p 2016 kjhlj", "Valana la Legende")]
        [TestCase("Valana la Legende TRUEFRENCH BluRay 720p 2016 kjhlj", "Valana la Legende")]
        [TestCase("Mission Impossible: Rogue Nation (2015)�[XviD - Ita Ac3 - SoftSub Ita]azione, spionaggio, thriller *Prima Visione* Team mulnic Tom Cruise", "Mission Impossible Rogue Nation")]
        [TestCase("Scary.Movie.2000.FRENCH..BluRay.-AiRLiNE", "Scary Movie")]
        [TestCase("My Movie 1999 German Bluray", "My Movie")]
        [TestCase("Leaving Jeruselem by Railway (1897) [DVD].mp4", "Leaving Jeruselem by Railway")]
        [TestCase("Climax.2018.1080p.AMZN.WEB-DL.DD5.1.H.264-NTG", "Climax")]
        [TestCase("Movie.Title.Imax.2018.1080p.AMZN.WEB-DL.DD5.1.H.264-NTG", "Movie Title")]
        [TestCase("World.War.Z.EXTENDED.2013.German.DL.1080p.BluRay.AVC-XANOR", "World War Z")]
        [TestCase("World.War.Z.2.EXTENDED.2013.German.DL.1080p.BluRay.AVC-XANOR", "World War Z 2")]
        [TestCase("G.I.Joe.Retaliation.2013.THEATRiCAL.COMPLETE.BLURAY-GLiMMER", "G.I. Joe Retaliation")]
        [TestCase("www.Torrenting.org - Revenge.2008.720p.X264-DIMENSION", "Revenge")]
        public void should_parse_movie_title(string postTitle, string title)
        {
            Parser.Parser.ParseMovieTitle(postTitle).MovieTitle.Should().Be(title);
        }

        [TestCase("Avatar.Aufbruch.nach.Pandora.Extended.2009.German.DTS.720p.BluRay.x264-SoW", "Avatar Aufbruch nach Pandora", "Extended", 2009)]
        [TestCase("Drop.Zone.1994.German.AC3D.DL.720p.BluRay.x264-KLASSiGERHD", "Drop Zone", "", 1994)]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate", "Kick Ass 2", "", 2013)]
        [TestCase("Paradise.Hills.2019.German.DL.AC3.Dubbed.1080p.BluRay.x264-muhHD", "Paradise Hills", "", 2019)]
        [TestCase("96.Hours.Taken.3.EXTENDED.2014.German.DL.1080p.BluRay.x264-ENCOUNTERS", "96 Hours Taken 3", "EXTENDED", 2014)]
        [TestCase("World.War.Z.EXTENDED.CUT.2013.German.DL.1080p.BluRay.x264-HQX", "World War Z", "EXTENDED CUT", 2013)]
        [TestCase("Sin.City.2005.RECUT.EXTENDED.German.DL.1080p.BluRay.x264-DETAiLS", "Sin City", "RECUT EXTENDED", 2005)]
        [TestCase("2.Tage.in.L.A.1996.GERMAN.DL.720p.WEB.H264-SOV", "2 Tage in L.A.", "", 1996)]
        [TestCase("8.2019.GERMAN.720p.BluRay.x264-UNiVERSUM", "8", "", 2019)]
        [TestCase("Life.Partners.2014.German.DL.PAL.DVDR-ETM", "Life Partners", "", 2014)]
        [TestCase("Joe.Dreck.2.EXTENDED.EDITION.2015.German.DL.PAL.DVDR-ETM", "Joe Dreck 2", "EXTENDED EDITION", 2015)]
        [TestCase("Rango.EXTENDED.2011.HDRip.AC3.German.XviD-POE", "Rango", "EXTENDED", 2011)]

        //Special cases (see description)
        [TestCase("Die.Klasse.von.1999.1990.German.720p.HDTV.x264-NORETAiL", "Die Klasse von 1999", "", 1990, Description = "year in the title")]
        [TestCase("Suicide.Squad.2016.EXTENDED.German.DL.AC3.BDRip.x264-hqc", "Suicide Squad", "EXTENDED", 2016, Description = "edition after year")]
        [TestCase("Knight.and.Day.2010.Extended.Cut.German.DTS.DL.720p.BluRay.x264-HDS", "Knight and Day", "Extended Cut", 2010, Description = "edition after year")]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", "Der Soldat James", "", 1998, Description = "year at the end")]
        [TestCase("Der.Hobbit.Eine.Unerwartete.Reise.Extended.German.720p.BluRay.x264-EXQUiSiTE", "Der Hobbit Eine Unerwartete Reise", "Extended", 0, Description = "no year & edition")]
        [TestCase("Wolverine.Weg.des.Kriegers.EXTENDED.German.720p.BluRay.x264-EXQUiSiTE", "Wolverine Weg des Kriegers", "EXTENDED", 0, Description = "no year & edition")]
        [TestCase("Die.Unfassbaren.Now.You.See.Me.EXTENDED.German.DTS.720p.BluRay.x264-RHD", "Die Unfassbaren Now You See Me", "EXTENDED", 0, Description = "no year & edition")]
        [TestCase("Die Unfassbaren Now You See Me EXTENDED German DTS 720p BluRay x264-RHD", "Die Unfassbaren Now You See Me", "EXTENDED", 0, Description = "no year & edition & without dots")]
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

        [TestCase("(1995) Ghost in the Shell", "Ghost in the Shell")]
        public void should_parse_movie_folder_name(string postTitle, string title)
        {
            Parser.Parser.ParseMovieTitle(postTitle, true).MovieTitle.Should().Be(title);
        }

        [TestCase("1941.1979.EXTENDED.720p.BluRay.X264-AMIABLE", 1979)]
        [TestCase("Valana la Legende FRENCH BluRay 720p 2016 kjhlj", 2016)]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", 1998)]
        [TestCase("Leaving Jeruselem by Railway (1897) [DVD].mp4", 1897)]
        public void should_parse_movie_year(string postTitle, int year)
        {
            Parser.Parser.ParseMovieTitle(postTitle).Year.Should().Be(year);
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
        [TestCase("Loving.Pablo.2018.TS.FRENCH.MD.x264-DROGUERiE", "")]
        [TestCase("Uncut.Gems.2019.720p.BluRay.x264-YOL0W", "")]
        [TestCase("Directors.Cut.German.2006.COMPLETE.PAL.DVDR-LoD", "")]
        public void should_parse_edition(string postTitle, string edition)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle);
            parsed.Edition.Should().Be(edition);
        }

        [TestCase("The.Italian.Job.2008.720p.BluRay.X264-AMIABLE")]
        public void should_not_parse_wrong_language_in_title(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count().Should().Be(1);
            parsed.Languages.First().Should().Be(Language.Unknown);
        }

        [TestCase("The.Purge.3.Election.Year.2016.German.DTS.DL.720p.BluRay.x264-MULTiPLEX")]
        public void should_not_parse_multi_language_in_releasegroup(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count().Should().Be(1);
            parsed.Languages.First().Should().Be(Language.German);
        }

        [TestCase("The.Purge.3.Election.Year.2016.German.Multi.DTS.DL.720p.BluRay.x264-MULTiPLEX")]
        public void should_parse_multi_language(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle, true);
            parsed.Languages.Count().Should().Be(2);
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
