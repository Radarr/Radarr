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
        [TestCase("Pulp.Fiction.1994.English.1080p.XviD-LOL")]
        public void should_parse_language_english(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.English);
        }

        [TestCase("The Danish Girl 2015")]
        [TestCase("Fantastic.Beasts.The.Crimes.Of.Grindelwald.2018.2160p.WEBRip.x265.10bit.HDR.DD5.1-GASMASK")]
        public void should_parse_language_unknown(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Unknown);
        }

        [TestCase("Pulp.Fiction.1994.French.1080p.XviD-LOL")]
        public void should_parse_language_french(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.French);
        }

        [TestCase("E.T. the Extra-Terrestrial.1982.Ger.Eng.AC3.DL.BDRip.x264-iNCEPTiON")]
        public void should_parse_language_english_german(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().Contain(Language.German);
            result.Languages.Should().Contain(Language.English);
        }

        [TestCase("Pulp.Fiction.1994.Spanish.1080p.XviD-LOL")]
        public void should_parse_language_spanish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Spanish);
        }

        [TestCase("Pulp.Fiction.1994.German.1080p.XviD-LOL")]
        public void should_parse_language_german(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.German);
        }

        [TestCase("Pulp.Fiction.1994.Italian.1080p.XviD-LOL")]
        public void should_parse_language_italian(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Italian);
        }

        [TestCase("Pulp.Fiction.1994.Danish.1080p.XviD-LOL")]
        public void should_parse_language_danish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Danish);
        }

        [TestCase("Pulp.Fiction.1994.Dutch.1080p.XviD-LOL")]
        public void should_parse_language_dutch(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Dutch);
        }

        [TestCase("Pulp.Fiction.1994.Japanese.1080p.XviD-LOL")]
        public void should_parse_language_japanese(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Japanese);
        }

        [TestCase("Pulp.Fiction.1994.Icelandic.1080p.XviD-LOL")]
        public void should_parse_language_icelandic(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Icelandic);
        }

        [TestCase("Pulp.Fiction.1994.Chinese.1080p.XviD-LOL")]
        public void should_parse_language_chinese(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Chinese);
        }

        [TestCase("Pulp.Fiction.1994.Russian.1080p.XviD-LOL")]
        public void should_parse_language_russian(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Russian);
        }

        [TestCase("Pulp.Fiction.1994.Polish.1080p.XviD-LOL")]
        public void should_parse_language_polish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Polish);
        }

        [TestCase("Pulp.Fiction.1994.Vietnamese.1080p.XviD-LOL")]
        public void should_parse_language_vietnamese(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Vietnamese);
        }

        [TestCase("Pulp.Fiction.1994.Swedish.1080p.XviD-LOL")]
        public void should_parse_language_swedish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Swedish);
        }

        [TestCase("Pulp.Fiction.1994.Norwegian.1080p.XviD-LOL")]
        public void should_parse_language_norwegian(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Norwegian);
        }

        [TestCase("Pulp.Fiction.1994.Finnish.1080p.XviD-LOL")]
        public void should_parse_language_finnish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Finnish);
        }

        [TestCase("Pulp.Fiction.1994.Turkish.1080p.XviD-LOL")]
        public void should_parse_language_turkish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Turkish);
        }

        [TestCase("Pulp.Fiction.1994.Portuguese.1080p.XviD-LOL")]
        public void should_parse_language_portuguese(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Portuguese);
        }

        [TestCase("Pulp.Fiction.1994.Flemish.1080p.XviD-LOL")]
        public void should_parse_language_flemish(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Flemish);
        }

        [TestCase("Pulp.Fiction.1994.Greek.1080p.XviD-LOL")]
        public void should_parse_language_greek(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Greek);
        }

        [TestCase("Pulp.Fiction.1994.Korean.1080p.XviD-LOL")]
        public void should_parse_language_korean(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Korean);
        }

        [TestCase("Pulp.Fiction.1994.Hungarian.1080p.XviD-LOL")]
        public void should_parse_language_hungarian(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Hungarian);
        }

        [TestCase("Pulp.Fiction.1994.Hebrew.1080p.XviD-LOL")]
        public void should_parse_language_hebrew(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Hebrew);
        }

        [TestCase("Pulp.Fiction.1994.AC3.LT.EN-CNN")]
        public void should_parse_language_lithuanian(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Lithuanian);
        }

        [TestCase("Pulp.Fiction.1994.CZ.1080p.XviD-LOL")]
        public void should_parse_language_czech(string postTitle)
        {
            var result = Parser.Parser.ParseMovieTitle(postTitle, true);

            result.Languages.Should().BeEquivalentTo(Language.Czech);
        }

        [TestCase("Pulp Fiction.en.sub")]
        [TestCase("Pulp Fiction.eng.sub")]
        [TestCase("Pulp.Fiction.eng.forced.sub")]
        [TestCase("Pulp-Fiction-eng-forced.sub")]
        public void should_parse_subtitle_language(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.English);
        }

        [TestCase("Pulp Fiction.sub")]
        public void should_parse_subtitle_language_unknown(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.Unknown);
        }
    }
}
