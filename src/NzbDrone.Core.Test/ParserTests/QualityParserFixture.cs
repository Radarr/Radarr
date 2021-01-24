using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]

    public class QualityParserFixture : CoreTest
    {
        public static object[] SelfQualityParserCases =
        {
            new object[] { Quality.MP3_320 },
            new object[] { Quality.FLAC },
            new object[] { Quality.EPUB },
            new object[] { Quality.MOBI },
            new object[] { Quality.AZW3 }
        };

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]")]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK")]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]")]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)")]
        [TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]")]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps")]
        [TestCase("Some Song [192][2014][MP3]")]
        [TestCase("Other Song (192)[2014][MP3]")]
        [TestCase("Caetano Veloso Discografia Completa MP3 @256")]
        [TestCase("Jake Bugg - Jake Bugg (Book) [2012] {MP3 256 kbps}")]
        [TestCase("Clean Bandit - New Eyes [2014] [Mp3-256]-V3nom [GLT]")]
        [TestCase("PJ Harvey - Let England Shake [mp3-256-2011][trfkad]")]
        [TestCase("Childish Gambino - Awaken, My Love Book 2016 mp3 320 Kbps")]
        [TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download")]
        [TestCase("Sia - This Is Acting (Standard Edition) [2016-Web-MP3-V0(VBR)]")]
        [TestCase("Mount Eerie - A Crow Looked at Me (2017) [MP3 V0 VBR)]")]
        [TestCase("Queen - The Ultimate Best Of Queen(2011)[mp3]")]
        [TestCase("Maroon 5 Ft Kendrick Lamar -Dont Wanna Know MP3 2016")]
        public void should_parse_mp3_quality(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.MP3_320);
        }

        [TestCase("Kendrick Lamar - DAMN (2017) FLAC")]
        [TestCase("Alicia Keys - Vault Playlist Vol. 1 (2017) [FLAC CD]")]
        [TestCase("Gorillaz - Humanz (Deluxe) - lossless FLAC Tracks - 2017 - CDrip")]
        [TestCase("David Bowie - Blackstar (2016) [FLAC]")]
        [TestCase("The Cure - Greatest Hits (2001) FLAC Soup")]
        [TestCase("Slowdive- Souvlaki (FLAC)")]
        [TestCase("John Coltrane - Kulu Se Mama (1965) [EAC-FLAC]")]
        [TestCase("The Rolling Stones - The Very Best Of '75-'94 (1995) {FLAC}")]
        [TestCase("Migos-No_Label_II-CD-FLAC-2014-FORSAKEN")]
        [TestCase("ADELE 25 CD FLAC 2015 PERFECT")]
        public void should_parse_flac_quality(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.FLAC);
        }

        // Flack doesn't get match for 'FLAC' quality
        [TestCase("Roberta Flack 2006 - The Very Best of")]
        public void should_not_parse_flac_quality(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.Unknown);
        }

        [TestCase("The Chainsmokers & Coldplay - Something Just Like This")]
        [TestCase("Frank Ocean Blonde 2016")]
        public void quality_parse(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.Unknown);
        }

        [Test]
        [TestCaseSource(nameof(SelfQualityParserCases))]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = string.Format("Some book [{0}]", quality.Name);
            var result = QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }

        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT")]
        public void should_parse_quality_from_name(string title)
        {
            QualityParser.ParseQuality(title).QualityDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [Test]
        public void should_parse_null_quality_description_as_unknown()
        {
            QualityParser.ParseCodec(null, null).Should().Be(Codec.Unknown);
        }

        [TestCase("Author Title - Book Title 2017 REPACK FLAC aAF", true)]
        [TestCase("Author Title - Book Title 2017 RERIP FLAC aAF", true)]
        [TestCase("Author Title - Book Title 2017 PROPER FLAC aAF", false)]
        public void should_be_able_to_parse_repack(string title, bool isRepack)
        {
            var result = QualityParser.ParseQuality(title);
            result.Revision.Version.Should().Be(2);
            result.Revision.IsRepack.Should().Be(isRepack);
        }

        private void ParseAndVerifyQuality(string name, string desc, int bitrate, Quality quality, int sampleSize = 0)
        {
            var result = QualityParser.ParseQuality(name);
            result.Quality.Should().Be(quality);
        }
    }
}
