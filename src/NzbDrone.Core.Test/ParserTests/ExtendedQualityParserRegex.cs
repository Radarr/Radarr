using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ExtendedQualityParserRegex : CoreTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Chuck.S04E05.HDTV.XviD-LOL", 0)]
        [TestCase("Gold.Rush.S04E05.Garnets.or.Gold.REAL.REAL.PROPER.HDTV.x264-W4F", 2)]
        [TestCase("Chuck.S03E17.REAL.PROPER.720p.HDTV.x264-ORENJI-RP", 1)]
        [TestCase("Covert.Affairs.S05E09.REAL.PROPER.HDTV.x264-KILLERS", 1)]
        [TestCase("Mythbusters.S14E01.REAL.PROPER.720p.HDTV.x264-KILLERS", 1)]
        [TestCase("Orange.Is.the.New.Black.s02e06.real.proper.720p.webrip.x264-2hd", 0)]
        [TestCase("Top.Gear.S21E07.Super.Duper.Real.Proper.HDTV.x264-FTP", 0)]
        [TestCase("Top.Gear.S21E07.PROPER.HDTV.x264-RiVER-RP", 0)]
        [TestCase("House.S07E11.PROPER.REAL.RERIP.1080p.BluRay.x264-TENEIGHTY", 1)]
        [TestCase("[MGS] - Kuragehime - Episode 02v2 - [D8B6C90D]", 0)]
        [TestCase("[Hatsuyuki] Tokyo Ghoul - 07 [v2][848x480][23D8F455].avi", 0)]
        [TestCase("[DeadFish] Barakamon - 01v3 [720p][AAC]", 0)]
        [TestCase("[DeadFish] Momo Kyun Sword - 01v4 [720p][AAC]", 0)]
        [TestCase("The Real Housewives of Some Place - S01E01 - Why are we doing this?", 0)]
        public void should_parse_reality_from_title(string title, int reality)
        {
            QualityParser.ParseQuality(title).Revision.Real.Should().Be(reality);
        }

        [TestCase("Chuck.S04E05.HDTV.XviD-LOL", 1)]
        [TestCase("Gold.Rush.S04E05.Garnets.or.Gold.REAL.REAL.PROPER.HDTV.x264-W4F", 2)]
        [TestCase("Chuck.S03E17.REAL.PROPER.720p.HDTV.x264-ORENJI-RP", 2)]
        [TestCase("Covert.Affairs.S05E09.REAL.PROPER.HDTV.x264-KILLERS", 2)]
        [TestCase("Mythbusters.S14E01.REAL.PROPER.720p.HDTV.x264-KILLERS", 2)]
        [TestCase("Orange.Is.the.New.Black.s02e06.real.proper.720p.webrip.x264-2hd", 2)]
        [TestCase("Top.Gear.S21E07.Super.Duper.Real.Proper.HDTV.x264-FTP", 2)]
        [TestCase("Top.Gear.S21E07.PROPER.HDTV.x264-RiVER-RP", 2)]
        [TestCase("House.S07E11.PROPER.REAL.RERIP.1080p.BluRay.x264-TENEIGHTY", 2)]
        [TestCase("[MGS] - Kuragehime - Episode 02v2 - [D8B6C90D]", 2)]
        [TestCase("[Hatsuyuki] Tokyo Ghoul - 07 [v2][848x480][23D8F455].avi", 2)]
        [TestCase("[DeadFish] Barakamon - 01v3 [720p][AAC]", 3)]
        [TestCase("[DeadFish] Momo Kyun Sword - 01v4 [720p][AAC]", 4)]
        [TestCase("[Vivid-Asenshi] Akame ga Kill - 04v2 [266EE983]", 2)]
        [TestCase("[Vivid-Asenshi] Akame ga Kill - 03v2 [66A05817]", 2)]
        [TestCase("[Vivid-Asenshi] Akame ga Kill - 02v2 [1F67AB55]", 2)]
        public void should_parse_version_from_title(string title, int version)
        {
            QualityParser.ParseQuality(title).Revision.Version.Should().Be(version);
        }

        [TestCase("Deadpool 2016 2160p 4K UltraHD BluRay DTS-HD MA 7 1 x264-Whatevs", 19)]
        [TestCase("Deadpool 2016 2160p 4K UltraHD DTS-HD MA 7 1 x264-Whatevs", 16)]
        [TestCase("Deadpool 2016 4K 2160p UltraHD BluRay AAC2 0 HEVC x265", 19)]
        [TestCase("The Revenant 2015 2160p UHD BluRay DTS x264-Whatevs", 19)]
        [TestCase("The Revenant 2015 2160p UHD BluRay FLAC 7 1 x264-Whatevs", 19)]
        [TestCase("The Martian 2015 2160p Ultra HD BluRay DTS-HD MA 7 1 x264-Whatevs", 19)]
        [TestCase("Into the Inferno 2016 2160p Netflix WEBRip DD5 1 x264-Whatevs", 18)]
        public void should_parse_ultrahd_from_title(string title, int version)
        {
            var parsed = QualityParser.ParseQuality(title);
            parsed.Quality.Resolution.Should().Be(Resolution.R2160P);
        }
    }
}
