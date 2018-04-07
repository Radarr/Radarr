using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
        [TestCase("Alien.Ant.Farm-truAnt.2009.English.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.Germany.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.FLAC.XviD-LOL")]
        [TestCase("Two.Greedy.Italians.S01E01.The.Family.720p.HDTV.x264-FTP")]
        [TestCase("The.Trip.To.Italy.S02E01.720p.HDTV.x264-TLA")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.en.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.eng.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.English.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.english.sub")]
        public void should_parse_language_english(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Should().Be(Language.English);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.FLAC.XviD-LOL")]
        public void should_parse_subtitle_language_unknown(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.Unknown);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.French.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.VOSTFR.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.FR.FLAC.XviD-LOL")]
        public void should_parse_language_french(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.French.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Spanish.FLAC.XviD-LOL")]
        public void should_parse_language_spanish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Spanish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.German.FLAC.XviD-LOL")]
        public void should_parse_language_german(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.German.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Italian.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.ita.FLAC.XviD-LOL")]
        public void should_parse_language_italian(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Italian.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Danish.FLAC.XviD-LOL")]
        public void should_parse_language_danish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Danish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Dutch.FLAC.XviD-LOL")]
        public void should_parse_language_dutch(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Dutch.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Japanese.FLAC.XviD-LOL")]
        public void should_parse_language_japanese(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Japanese.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Cantonese.FLAC.XviD-LOL")]
        public void should_parse_language_cantonese(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Cantonese.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Mandarin.FLAC.XviD-LOL")]
        public void should_parse_language_mandarin(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Mandarin.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Korean.FLAC.XviD-LOL")]
        public void should_parse_language_korean(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Korean.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Russian.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.Rus.Eng.FLAC.XviD-LOL")]
        public void should_parse_language_russian(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Russian.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Polish.FLAC.XviD-LOL")]
        public void should_parse_language_polish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Polish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Vietnamese.FLAC.XviD-LOL")]
        public void should_parse_language_vietnamese(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Vietnamese.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Swedish.FLAC.XviD-LOL")]
        public void should_parse_language_swedish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Swedish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Norwegian.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.Nordic.FLAC.XviD-LOL")]
        public void should_parse_language_norwegian(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Norwegian.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Finnish.FLAC.XviD-LOL")]
        public void should_parse_language_finnish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Finnish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Turkish.FLAC.XviD-LOL")]
        public void should_parse_language_turkish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Turkish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Portuguese.FLAC.XviD-LOL")]
        public void should_parse_language_portuguese(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Portuguese.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.FLEMISH.FLAC.XviD-LOL")]
        public void should_parse_language_flemish(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Flemish.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.Greek.FLAC.XviD-LOL")]
        public void should_parse_language_greek(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Greek.Id);
        }

        [TestCase("Alien.Ant.Farm-truAnt.2009.HUNDUB.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.ENG.HUN.FLAC.XviD-LOL")]
        [TestCase("Alien.Ant.Farm-truAnt.2009.HUN.FLAC.XviD-LOL")]
        public void should_parse_language_hungarian(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Hungarian.Id);
        }

        [Ignore("Not Implemented")]
        [TestCase("Avatar.The.Last.Airbender.S01-03.DVDRip.HebDub")]
        public void should_parse_language_hebrew(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Hebrew.Id);
        }

        [Ignore("Not Implemented")]
        [TestCase("Prison.Break.S05E01.WEBRip.x264.AC3.LT.EN-CNN")]
        public void should_parse_language_lithuanian(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Lithuanian.Id);
        }

        [Ignore("Not Implemented")]
        [TestCase("The.​Walking.​Dead.​S07E11.​WEB Rip.​XviD.​Louige-​CZ.​EN.​5.​1")]
        public void should_parse_language_czech(string postTitle)
        {
            var result = Parser.Parser.ParseAlbumTitle(postTitle);
            result.Language.Id.Should().Be(Language.Czech.Id);
        }
    }
}
