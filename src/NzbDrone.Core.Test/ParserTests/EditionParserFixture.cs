using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class EditionParserFixture : CoreTest
    {
        [TestCase("Movie Title 2012 Directors Cut", "Directors Cut")]
        [TestCase("Movie Title 1999 (Despecialized).mkv", "Despecialized")]
        [TestCase("Movie Title.2012.(Special.Edition.Remastered).[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Movie Title 2012 Extended", "Extended")]
        [TestCase("Movie Title 2012 Extended Directors Cut Fan Edit", "Extended Directors Cut Fan Edit")]
        [TestCase("Movie Title 2012 Director's Cut", "Director's Cut")]
        [TestCase("Movie Title 2012 Directors Cut", "Directors Cut")]
        [TestCase("Movie Title.2012.(Extended.Theatrical.Version.IMAX).BluRay.1080p.2012.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("2021 A Movie (1968) Director's Cut .mkv", "Director's Cut")]
        [TestCase("2021 A Movie 1968 (Extended Directors Cut FanEdit)", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 2012 Directors.mkv", "Directors")]
        [TestCase("Movie 2049 Director's Cut.mkv", "Director's Cut")]
        [TestCase("Movie Title 2012 50th Anniversary Edition.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2012 2in1.mkv", "2in1")]
        [TestCase("Movie 2012 IMAX.mkv", "IMAX")]
        [TestCase("Movie 2012 Restored.mkv", "Restored")]
        [TestCase("Movie Title.Special.Edition.Fan Edit.2012..BRRip.x264.AAC-m2g", "Special Edition Fan Edit")]
        [TestCase("Movie Title (Despecialized) 1999.mkv", "Despecialized")]
        [TestCase("Movie Title.(Special.Edition.Remastered).2012.[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Movie Title Extended 2012", "Extended")]
        [TestCase("Movie Title Extended Directors Cut Fan Edit 2012", "Extended Directors Cut Fan Edit")]
        [TestCase("Movie Title Director's Cut 2012", "Director's Cut")]
        [TestCase("Movie Title Directors Cut 2012", "Directors Cut")]
        [TestCase("Movie Title.(Extended.Theatrical.Version.IMAX).2012.BluRay.1080p.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("Movie Director's Cut (1968).mkv", "Director's Cut")]
        [TestCase("2021 A Movie (Extended Directors Cut FanEdit) 1968 Bluray 1080p", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 Directors 2012.mkv", "Directors")]
        [TestCase("Movie Director's Cut 2049.mkv", "Director's Cut")]
        [TestCase("Movie Title 50th Anniversary Edition 2012.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2in1 2012.mkv", "2in1")]
        [TestCase("Movie IMAX 2012.mkv", "IMAX")]
        [TestCase("Fake Movie Final Cut 2016", "Final Cut")]
        [TestCase("Fake Movie 2016 Final Cut ", "Final Cut")]
        [TestCase("My Movie GERMAN Extended Cut 2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut.2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut", "Extended Cut")]
        [TestCase("My.Movie.Assembly.Cut.1992.REPACK.1080p.BluRay.DD5.1.x264-Group", "Assembly Cut")]
        [TestCase("Movie.1987.Ultimate.Hunter.Edition.DTS-HD.DTS.MULTISUBS.1080p.BluRay.x264.HQ-TUSAHD", "Ultimate Hunter Edition")]
        [TestCase("Movie.1950.Diamond.Edition.1080p.BluRay.x264-nikt0", "Diamond Edition")]
        [TestCase("Movie.Title.1990.Ultimate.Rekall.Edition.NORDiC.REMUX.1080p.BluRay.AVC.DTS-HD.MA5.1-TWA", "Ultimate Rekall Edition")]
        [TestCase("Movie.Title.1971.Signature.Edition.1080p.BluRay.FLAC.2.0.x264-TDD", "Signature Edition")]
        [TestCase("Movie.1979.The.Imperial.Edition.BluRay.720p.DTS.x264-CtrlHD", "Imperial Edition")]
        public void should_parse_edition(string postTitle, string edition)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle);
            parsed.Edition.Should().Be(edition);
        }

        [TestCase("Movie.Holiday.Special.1978.DVD.REMUX.DD.2.0-ViETNAM")]
        [TestCase("Directors.Cut.German.2006.COMPLETE.PAL.DVDR-LoD")]
        [TestCase("Movie Impossible: Rogue Movie 2012 Bluray")]
        [TestCase("Loving.Movie.2018.TS.FRENCH.MD.x264-DROGUERiE")]
        [TestCase("Uncut.Movie.2019.720p.BluRay.x264-YOL0W")]
        [TestCase("The.Christmas.Edition.1941.720p.HDTV.x264-CRiMSON")]
        public void should_not_parse_edition(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle);
            parsed.Edition.Should().Be("");
        }
    }
}
