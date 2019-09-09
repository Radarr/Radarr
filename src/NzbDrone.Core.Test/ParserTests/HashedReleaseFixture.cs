using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class HashedReleaseFixture : CoreTest
    {
        public static object[] HashedReleaseParserCases =
        {
            new object[]
            {
                @"C:\Test\Some.Hashed.Release.(256kbps)-Mercury\0e895c37245186812cb08aab1529cf8ee389dd05.mp3".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.MP3_256,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test-[256]\0e895c37245186812cb08aab1529cf8ee389dd05\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mp3".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.MP3_256,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Fake.Dir.S01E01-Test\yrucreM-462.H.0.2CAA.LD-BEW.p027.10E10S.esaeleR.dehsaH.emoS.mp3".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.MP3_256,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Fake.Dir.S01E01-Test\yrucreM-LN 1.5DD LD-BEW P0801 10E10S esaeleR dehsaH emoS.mp3".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.MP3_256,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Weeds.S01E10.DVDRip.XviD-Lidarr\AHFMZXGHEWD660.mp3".AsOsAgnostic(),
                "Weeds",
                Quality.MP3_256,
                "Lidarr"
            },
            new object[]
            {
                @"C:\Test\Deadwood.S02E12.1080p.BluRay.x264-Lidarr\Backup_72023S02-12.mp3".AsOsAgnostic(),
                "Deadwood",
                Quality.MP3_256,
                null
            },
            new object[]
            {
                @"C:\Test\Grimm S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\123.mp3".AsOsAgnostic(),
                "Grimm",
                Quality.MP3_256,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\Grimm S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\abc.mp3".AsOsAgnostic(),
                "Grimm",
                Quality.MP3_256,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\Grimm S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\b00bs.mp3".AsOsAgnostic(),
                "Grimm",
                Quality.MP3_256,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\The.Good.Wife.S02E23.720p.HDTV.x264-NZBgeek/cgajsofuejsa501.mp3".AsOsAgnostic(),
                "The Good Wife",
                Quality.MP3_256,
                "NZBgeek"
            }
        };

        [Test, TestCaseSource(nameof(HashedReleaseParserCases))]
        [Ignore("Hashed code is not currently called with track parsing")]
        public void should_properly_parse_hashed_releases(string path, string title, Quality quality, string releaseGroup)
        {
            var result = Parser.Parser.ParseMusicPath(path);
            //result.SeriesTitle.Should().Be(title);
            result.Quality.Quality.Should().Be(quality);
        }
    }
}
