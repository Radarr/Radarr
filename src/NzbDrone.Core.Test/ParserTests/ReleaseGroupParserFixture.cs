using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-ENTiTLED", "ENTiTLED")]
        [TestCase("[ www.Torrenting.com ] - Olafur.Arnalds-Remember-WEB-2018-ENTiTLED", "ENTiTLED")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-ENTiTLED [eztv]-[rarbg.com]", "ENTiTLED")]
        [TestCase("7s-atlantis-128.mp3", null)]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-ENTiTLED-Pre", "ENTiTLED")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-ENTiTLED-postbot", "ENTiTLED")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-ENTiTLED-xpost", "ENTiTLED")]

        //[TestCase("", "")]
        public void should_parse_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [Test]
        [Ignore("Track name parsing needs to be worked on")]
        public void should_not_include_extension_in_release_group()
        {
            const string path = @"C:\Test\Doctor.Who.2005.s01e01.internal.bdrip.x264-archivist.mkv";

            Parser.Parser.ParseMusicPath(path).ReleaseGroup.Should().Be("archivist");
        }

        [TestCase("Olafur.Arnalds-Remember-WEB-2018-SKGTV English", "SKGTV")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-SKGTV_English", "SKGTV")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-SKGTV.English", "SKGTV")]

        //[TestCase("", "")]
        public void should_not_include_language_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Olafur.Arnalds-Remember-WEB-2018-EVL-RP", "EVL")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-EVL-RP-RP", "EVL")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-EVL-Obfuscated", "EVL")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-xHD-NZBgeek", "xHD")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-DIMENSION-NZBgeek", "DIMENSION")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-xHD-1", "xHD")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-DIMENSION-1", "DIMENSION")]
        [TestCase("Olafur.Arnalds-Remember-WEB-2018-EVL-Scrambled", "EVL")]
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
