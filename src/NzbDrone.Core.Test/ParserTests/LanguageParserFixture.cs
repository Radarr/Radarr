using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", Language.English)]
        [TestCase("Castle.2009.S01E14.French.HDTV.XviD-LOL", Language.French)]
        [TestCase("Ouija.Origin.of.Evil.2016.MULTi.TRUEFRENCH.1080p.BluRay.x264-MELBA", Language.French, Language.English)]
        [TestCase("Everest.2015.FRENCH.VFQ.BDRiP.x264-CNF30", Language.French)]
        [TestCase("Showdown.In.Little.Tokyo.1991.MULTI.VFQ.VFF.DTSHD-MASTER.1080p.BluRay.x264-ZombiE", Language.French, Language.English)]
        [TestCase("The.Polar.Express.2004.MULTI.VF2.1080p.BluRay.x264-PopHD", Language.French, Language.English)]
        [TestCase("Castle.2009.S01E14.Spanish.HDTV.XviD-LOL", Language.Spanish)]
        [TestCase("Castle.2009.S01E14.German.HDTV.XviD-LOL", Language.German)]
        [TestCase("Castle.2009.S01E14.Italian.HDTV.XviD-LOL", Language.Italian)]
        [TestCase("Castle.2009.S01E14.Danish.HDTV.XviD-LOL", Language.Danish)]
        [TestCase("Castle.2009.S01E14.Dutch.HDTV.XviD-LOL", Language.Dutch)]
        [TestCase("Castle.2009.S01E14.Japanese.HDTV.XviD-LOL", Language.Japanese)]
        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL", Language.Cantonese)]
        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL", Language.Mandarin)]
        [TestCase("Castle.2009.S01E14.Korean.HDTV.XviD-LOL", Language.Korean)]
        [TestCase("Castle.2009.S01E14.Russian.HDTV.XviD-LOL", Language.Russian)]
        [TestCase("Castle.2009.S01E14.Polish.HDTV.XviD-LOL", Language.Polish)]
        [TestCase("Castle.2009.S01E14.Vietnamese.HDTV.XviD-LOL", Language.Vietnamese)]
        [TestCase("Castle.2009.S01E14.Swedish.HDTV.XviD-LOL", Language.Swedish)]
        [TestCase("Castle.2009.S01E14.Norwegian.HDTV.XviD-LOL", Language.Norwegian)]
        [TestCase("Castle.2009.S01E14.Finnish.HDTV.XviD-LOL", Language.Finnish)]
        [TestCase("Castle.2009.S01E14.Turkish.HDTV.XviD-LOL", Language.Turkish)]
        [TestCase("Castle.2009.S01E14.Czech.HDTV.XviD-LOL", Language.Czech)]
        [TestCase("Castle.2009.S01E14.Portuguese.HDTV.XviD-LOL", Language.Portuguese)]
        [TestCase("Burn.Notice.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP", Language.German)]
        [TestCase("Revolution S01E03 No Quarter 2012 WEB-DL 720p Nordic-philipo mkv", Language.Norwegian)]
        [TestCase("Constantine.2014.S01E01.WEBRiP.H264.AAC.5.1-NL.SUBS", Language.Dutch)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUNDUB-LOL", Language.Hungarian)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.ENG.HUN-LOL", Language.Hungarian)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUN-LOL", Language.Hungarian)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.CZ-LOL", Language.Czech)]
        [TestCase("Passengers.2016.German.DL.AC3.Dubbed.1080p.WebHD.h264.iNTERNAL-PsO", Language.German)]
        [TestCase("Der.Soldat.James.German.Bluray.FuckYou.Pso.Why.cant.you.follow.scene.rules.1998", Language.German)]
        [TestCase("Passengers.German.DL.AC3.Dubbed..BluRay.x264-PsO", Language.German)]
        [TestCase("Valana la Legende FRENCH BluRay 720p 2016 kjhlj", Language.French)]
        [TestCase("Smurfs.​The.​Lost.​Village.​2017.​1080p.​BluRay.​HebDub.​x264-​iSrael",Language.Hebrew)]
        [TestCase("The Danish Girl 2015", Language.English)]
        [TestCase("Nocturnal Animals (2016) MULTi VFQ English [1080p] BluRay x264-PopHD", Language.English, Language.French)]
        [TestCase("Wonder.Woman.2017.720p.BluRay.DD5.1.x264-TayTO.CZ-FTU", Language.Czech)]
        public void should_parse_language(string postTitle, params Language[] languages)
        {
            var movieInfo = Parser.Parser.ParseMovieTitle(postTitle, true);
            var languageTitle = postTitle;
            if (movieInfo != null)
            {
                languageTitle = movieInfo.SimpleReleaseTitle;
            }
            var result = LanguageParser.ParseLanguages(languageTitle);
            result = LanguageParser.EnhanceLanguages(languageTitle, result);
            result.Should().BeEquivalentTo(languages);
        }

        [TestCase("2 Broke Girls - S01E01 - Pilot.en.sub", Language.English)]
        [TestCase("2 Broke Girls - S01E01 - Pilot.eng.sub", Language.English)]
        [TestCase("2 Broke Girls - S01E01 - Pilot.sub", Language.Unknown)]
        [TestCase("2 Broke Girls - S01E01 - Pilot.eng.forced.sub", Language.English)]
        [TestCase("2 Broke Girls - S01E01 - Pilot-eng-forced.sub", Language.English)]
        public void should_parse_subtitle_language(string fileName, Language language)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(language);
        }
    }
}
