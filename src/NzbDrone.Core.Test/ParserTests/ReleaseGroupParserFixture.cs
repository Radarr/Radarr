using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", "LOL")]
        [TestCase("Castle 2009 S01E14 English HDTV XviD LOL", null)]
        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER", null)]
        [TestCase("Punky.Brewster.S01.EXTRAS.DVDRip.XviD-RUNNER", "RUNNER")]
        [TestCase("2020.NZ.2011.12.02.PDTV.XviD-C4TV", "C4TV")]
        [TestCase("The.Office.S03E115.DVDRip.XviD-OSiTV", "OSiTV")]
        [TestCase("The Office - S01E01 - Pilot [HTDV-480p]", null)]
        [TestCase("The Office - S01E01 - Pilot [HTDV-720p]", null)]
        [TestCase("The Office - S01E01 - Pilot [HTDV-1080p]", null)]
        [TestCase("The.Walking.Dead.S04E13.720p.WEB-DL.AAC2.0.H.264-Cyphanix", "Cyphanix")]
        [TestCase("Arrow.S02E01.720p.WEB-DL.DD5.1.H.264.mkv", null)]
        [TestCase("Series Title S01E01 Episode Title", null)]
        [TestCase("The Colbert Report - 2014-06-02 - Thomas Piketty.mkv", null)]
        [TestCase("Real Time with Bill Maher S12E17 May 23, 2014.mp4", null)]
        [TestCase("Reizen Waes - S01E08 - Transistri\u00EB, Zuid-Osseti\u00EB en Abchazi\u00EB SDTV.avi", null)]
        [TestCase("Simpsons 10x11 - Wild Barts Cant Be Broken [rl].avi", null)]
        [TestCase("[ www.Torrenting.com ] - Revenge.S03E14.720p.HDTV.X264-DIMENSION", "DIMENSION")]
        [TestCase("Seed S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "2HD")]
        [TestCase("7s-atlantis-s02e01-720p.mkv", null)]
        [TestCase("The.Middle.720p.HEVC.x265-MeGusta-Pre", "MeGusta")]
        [TestCase("Haunted.Hayride.2018.720p.WEBRip.DDP5.1.x264-NTb-postbot", "NTb")]
        [TestCase("Haunted.Hayride.2018.720p.WEBRip.DDP5.1.x264-NTb-xpost", "NTb")]
        //[TestCase("", "")]
        public void should_parse_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        

        [TestCase("Marvels.Daredevil.S02E04.720p.WEBRip.x264-SKGTV English", "SKGTV")]
        [TestCase("Marvels.Daredevil.S02E04.720p.WEBRip.x264-SKGTV_English", "SKGTV")]
        [TestCase("Marvels.Daredevil.S02E04.720p.WEBRip.x264-SKGTV.English", "SKGTV")]
        //[TestCase("", "")]
        public void should_not_include_language_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("The.Longest.Mystery.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP", "EVL")]
        [TestCase("The.Longest.Mystery.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP-RP", "EVL")]
        [TestCase("The.Longest.Mystery.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-Obfuscated", "EVL")]
        [TestCase("Lost.S04E04.720p.BluRay.x264-xHD-NZBgeek", "xHD")]
        [TestCase("Blue.Bloods.S05E11.720p.HDTV.X264-DIMENSION-NZBgeek", "DIMENSION")]
        [TestCase("Lost.S04E04.720p.BluRay.x264-xHD-1", "xHD")]
        [TestCase("Blue.Bloods.S05E11.720p.HDTV.X264-DIMENSION-1", "DIMENSION")]
        [TestCase("saturday.night.live.s40e11.kevin.hart_sia.720p.hdtv.x264-w4f-sample.mkv", "w4f")]
        public void should_not_include_repost_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("[FFF] Invaders of the Rokujouma!! - S01E11 - Someday, With Them", "FFF")]
        [TestCase("[HorribleSubs] Invaders of the Rokujouma!! - S01E12 - Invasion Going Well!!", "HorribleSubs")]
        [TestCase("[Anime-Koi] Barakamon - S01E06 - Guys From Tokyo", "Anime-Koi")]
        [TestCase("[Anime-Koi] Barakamon - S01E07 - A High-Grade Fish", "Anime-Koi")]
        [TestCase("[Anime-Koi] Kami-sama Hajimemashita 2 - 01 [h264-720p][28D54E2C]", "Anime-Koi")]
        //[TestCase("Tokyo.Ghoul.02x01.013.HDTV-720p-Anime-Koi", "Anime-Koi")]
        //[TestCase("", "")]
        public void should_parse_anime_release_groups(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
