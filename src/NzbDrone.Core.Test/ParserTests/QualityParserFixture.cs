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
            new object[] {Quality.MP3_192},
            new object[] {Quality.MP3_VBR},
            new object[] {Quality.MP3_256},
            new object[] {Quality.MP3_320},
            new object[] {Quality.MP3_512},
            new object[] {Quality.FLAC},
        };

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]")]
        [TestCase("ATCQ - The Love Movement 1998 2CD 192kbps  RIP")]
        [TestCase("A Tribe Called Quest - The Love Movement 1998 2CD [192kbps] RIP")]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK")]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]")]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)")]
        [TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]")]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps")]
        [TestCase("Some Song [192][2014][MP3]")]
        [TestCase("Other Song (192)[2014][MP3]")]
        public void should_parse_mp3_192_quality(string title)
        {
            ParseAndVerifyQuality(title, Quality.MP3_192);
        }

        [TestCase("Beyoncé Lemonade [320] 2016 Beyonce Lemonade [320] 2016")]
        [TestCase("Childish Gambino - Awaken, My Love Album 2016 mp3 320 Kbps")]
        [TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download")]
        [TestCase("Ricardo Arjona - APNEA (Single 2014) (320 kbps)")]
        [TestCase("Kehlani - SweetSexySavage (Deluxe Edition) (2017) 320")]
        [TestCase("Anderson Paak - Malibu (320)(2016)")]
        public void should_parse_mp3_320_quality(string title)
        {
            ParseAndVerifyQuality(title, Quality.MP3_320);
        }


        [TestCase("Caetano Veloso Discografia Completa MP3 @256")]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT")]
        [TestCase("Ricky Martin - A Quien Quiera Escuchar (2015) 256 kbps [GloDLS]")]
        [TestCase("Jake Bugg - Jake Bugg (Album) [2012] {MP3 256 kbps}")]
        [TestCase("Milky Chance - Sadnecessary [256 Kbps] [M4A]")]
        [TestCase("Clean Bandit - New Eyes [2014] [Mp3-256]-V3nom [GLT]")]
        [TestCase("Armin van Buuren - A State Of Trance 810 (20.04.2017) 256 kbps")]
        [TestCase("PJ Harvey - Let England Shake [mp3-256-2011][trfkad]")]
        [TestCase("X-Men Soundtracks (2006-2014) AAC, 256 kbps")]
        [TestCase("Walk the Line Soundtrack (2005) [AAC, 256 kbps]")]
        public void should_parse_mp3_256_quality(string title)
        {
            ParseAndVerifyQuality(title, Quality.MP3_256);
        }

        [TestCase("Caetano Veloso Discografia Completa MP3 @512")]
        [TestCase("Walk the Line Soundtrack (2005) [AAC, 512 kbps]")]
        [TestCase("Emeli Sande Next To Me (512 Kbps)")]
        public void should_parse_mp3_512_quality(string title)
        {
            ParseAndVerifyQuality(title, Quality.MP3_512);
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
            ParseAndVerifyQuality(title, Quality.FLAC);
        }

        // Flack doesn't get match for 'FLAC' quality
        [TestCase("Roberta Flack 2006 - The Very Best of")]
        public void should_not_parse_flac_quality(string title)
        {
            ParseAndVerifyQuality(title, Quality.Unknown);
        }

        [TestCase("The Chainsmokers & Coldplay - Something Just Like This")]
        [TestCase("Frank Ocean Blonde 2016")]
        [TestCase("A - NOW Thats What I Call Music 96 (2017) [Mp3~Kbps]")]
        [TestCase("Queen - The Ultimate Best Of Queen(2011)[mp3]")]
        [TestCase("Maroon 5 Ft Kendrick Lamar -Dont Wanna Know MP3 2016")]
        public void quality_parse(string title)
        {
            ParseAndVerifyQuality(title, Quality.Unknown);
        }

        [Test, TestCaseSource(nameof(SelfQualityParserCases))]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = string.Format("Some album [{0}]", quality.Name);
            var result = QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }

        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT")]
        public void should_parse_quality_from_name(string title)
        {
            QualityParser.ParseQuality(title).QualitySource.Should().Be(QualitySource.Name);
        }

        [TestCase("01. Kanye West - Ultralight Beam.mp3")]
        [TestCase("01. Kanye West - Ultralight Beam.ogg")]
        [TestCase("01. Kanye West - Ultralight Beam.m4a")]
        public void should_parse_quality_from_extension(string title)
        {
            QualityParser.ParseQuality(title).QualitySource.Should().Be(QualitySource.Extension);
        }

        private void ParseAndVerifyQuality(string title, Quality quality)
        {
            var result = QualityParser.ParseQuality(title);
            result.Quality.Should().Be(quality);
        }
    }
}
