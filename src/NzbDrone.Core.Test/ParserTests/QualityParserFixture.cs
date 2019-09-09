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
            new object[] {Quality.MP3_VBR_V2},
            new object[] {Quality.WAV},
            new object[] {Quality.WMA},
            new object[] {Quality.AAC_192},
            new object[] {Quality.AAC_256},
            new object[] {Quality.AAC_320},
            new object[] {Quality.AAC_VBR},
            new object[] {Quality.ALAC},
            new object[] {Quality.FLAC},
        };

        [TestCase("", "MPEG Version 1 Audio, Layer 3", 96)]
        public void should_parse_mp3_96_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_096);
        }

        [TestCase("", "MPEG Version 1 Audio, Layer 3", 128)]
        public void should_parse_mp3_128_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_128);
        }

        [TestCase("", "MPEG Version 1 Audio, Layer 3", 160)]
        public void should_parse_mp3_160_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_160);
        }

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]", null, 0)]
        [TestCase("ATCQ - The Love Movement 1998 2CD 192kbps  RIP", null, 0)]
        [TestCase("A Tribe Called Quest - The Love Movement 1998 2CD [192kbps] RIP", null, 0)]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK", null, 0)]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]", null, 0)]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)", null, 0)]
        [TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]", null, 0)]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps", null, 0)]
        [TestCase("Some Song [192][2014][MP3]", null, 0)]
        [TestCase("Other Song (192)[2014][MP3]", null, 0)]
        [TestCase("", "MPEG Version 1 Audio, Layer 3", 192)]
        public void should_parse_mp3_192_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_192);
        }

        [TestCase("Caetano Veloso Discografia Completa MP3 @256", null, 0)]
        [TestCase("Ricky Martin - A Quien Quiera Escuchar (2015) 256 kbps [GloDLS]", null, 0)]
        [TestCase("Jake Bugg - Jake Bugg (Album) [2012] {MP3 256 kbps}", null, 0)]
        [TestCase("Clean Bandit - New Eyes [2014] [Mp3-256]-V3nom [GLT]", null, 0)]
        [TestCase("Armin van Buuren - A State Of Trance 810 (20.04.2017) 256 kbps", null, 0)]
        [TestCase("PJ Harvey - Let England Shake [mp3-256-2011][trfkad]", null, 0)]
        [TestCase("", "MPEG Version 1 Audio, Layer 3", 256)]
        public void should_parse_mp3_256_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_256);
        }

        [TestCase("Beyoncé Lemonade [320] 2016 Beyonce Lemonade [320] 2016", null, 0)]
        [TestCase("Childish Gambino - Awaken, My Love Album 2016 mp3 320 Kbps", null, 0)]
        [TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download", null, 0)]
        [TestCase("Ricardo Arjona - APNEA (Single 2014) (320 kbps)", null, 0)]
        [TestCase("Kehlani - SweetSexySavage (Deluxe Edition) (2017) 320", null, 0)]
        [TestCase("Anderson Paak - Malibu (320)(2016)", null, 0)]
        [TestCase("", "MPEG Version 1 Audio, Layer 3", 320)]
        public void should_parse_mp3_320_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_320);
        }

        [TestCase("Sia - This Is Acting (Standard Edition) [2016-Web-MP3-V0(VBR)]", null, 0)]
        [TestCase("Mount Eerie - A Crow Looked at Me (2017) [MP3 V0 VBR)]", null, 0)]
        public void should_parse_mp3_vbr_v0_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_VBR);
        }

        //TODO Parser should look at bitrate range for quality to determine level of VBR
        [TestCase("", "MPEG Version 1 Audio, Layer 3 VBR", 298)]
        [Ignore("Parser should look at bitrate range for quality to determine level of VBR")]
        public void should_parse_mp3_vbr_v2_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.MP3_VBR_V2);
        }

        [TestCase("Kendrick Lamar - DAMN (2017) FLAC", null, 0)]
        [TestCase("Alicia Keys - Vault Playlist Vol. 1 (2017) [FLAC CD]", null, 0)]
        [TestCase("Gorillaz - Humanz (Deluxe) - lossless FLAC Tracks - 2017 - CDrip", null, 0)]
        [TestCase("David Bowie - Blackstar (2016) [FLAC]", null, 0)]
        [TestCase("The Cure - Greatest Hits (2001) FLAC Soup", null, 0)]
        [TestCase("Slowdive- Souvlaki (FLAC)", null, 0)]
        [TestCase("John Coltrane - Kulu Se Mama (1965) [EAC-FLAC]", null, 0)]
        [TestCase("The Rolling Stones - The Very Best Of '75-'94 (1995) {FLAC}", null, 0)]
        [TestCase("Migos-No_Label_II-CD-FLAC-2014-FORSAKEN", null, 0)]
        [TestCase("ADELE 25 CD FLAC 2015 PERFECT", null, 0)]
        [TestCase("", "Flac Audio", 1057)]
        public void should_parse_flac_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.FLAC);
        }

        [TestCase("Beck.-.Guero.2005.[2016.Remastered].24bit.96kHz.LOSSLESS.FLAC", null, 0, 0)]
        [TestCase("[R.E.M - Lifes Rich Pageant(1986) [24bit192kHz 2016 Remaster]LOSSLESS FLAC]", null, 0, 0)]
        [TestCase("", "Flac Audio", 5057, 24)]
        public void should_parse_flac_24bit_quality(string title, string desc, int bitrate, int sampleSize)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.FLAC_24, sampleSize);
        }

        [TestCase("", "Microsoft WMA2 Audio", 218)]
        public void should_parse_wma_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.WMA);
        }

        [TestCase("", "PCM Audio", 1411)]
        public void should_parse_wav_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.WAV);
        }

        
        [TestCase("Chuck Berry Discography ALAC", null, 0)]
        [TestCase("A$AP Rocky - LONG LIVE A$AP Deluxe asap[ALAC]", null, 0)]
        [TestCase("", "MPEG-4 Audio (alac)", 0)]
        public void should_parse_alac_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.ALAC);
        }
        
        [TestCase("Stevie Ray Vaughan Discography (1981-1987) [APE]", null, 0)]
        [TestCase("Brain Ape - Rig it [2014][ape]", null, 0)]
        [TestCase("", "Monkey's Audio", 0)]
        public void should_parse_ape_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.APE);
        }

        [TestCase("Arctic Monkeys - AM {2013-Album}", null, 0)]
        [TestCase("Audio Adrinaline - Audio Adrinaline", null, 0)]
        [TestCase("Audio Adrinaline - Audio Adrinaline [Mixtape FLAC]", null, 0)]
        [TestCase("Brain Ape - Rig it [2014][flac]", null, 0)]
        [TestCase("Coil - The Ape Of Naples(2005) (FLAC)", null, 0)]
        public void should_not_parse_ape_quality(string title, string desc, int bitrate)
        {
            var result = QualityParser.ParseQuality(title, desc, bitrate);
            result.Quality.Should().NotBe(Quality.APE);
        }

        [TestCase("Opus - Drums Unlimited (1966) [Flac]", null, 0)]
        public void should_not_parse_opus_quality(string title, string desc, int bitrate)
        {
            var result = QualityParser.ParseQuality(title, desc, bitrate);
            result.Quality.Should().Be(Quality.FLAC);
        }

        [TestCase("Max Roach - Drums Unlimited (1966) [WavPack]", null, 0)]
        [TestCase("Roxette - Charm School(2011) (2CD) [WV]", null, 0)]
        [TestCase("", "WavPack", 0)]
        public void should_parse_wavpack_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.WAVPACK);
        }

        [TestCase("Milky Chance - Sadnecessary [256 Kbps] [M4A]", null, 0)]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT", null, 0)]
        [TestCase("X-Men Soundtracks (2006-2014) AAC, 256 kbps", null, 0)]
        [TestCase("The Weeknd - The Hills - Single[iTunes Plus AAC M4A]", null, 0)]
        [TestCase("Walk the Line Soundtrack (2005) [AAC, 256 kbps]", null, 0)]
        [TestCase("Firefly Soundtrack(2007 (2002-2003)) [AAC, 256 kbps VBR]", null, 0)]
        public void should_parse_aac_256_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.AAC_256);
        }

        [TestCase("", "MPEG-4 Audio (mp4a)", 320)]
        [TestCase("", "MPEG-4 Audio (drms)", 320)]
        public void should_parse_aac_320_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.AAC_320);
        }

        [TestCase("", "MPEG-4 Audio (mp4a)", 321)]
        [TestCase("", "MPEG-4 Audio (drms)", 321)]
        public void should_parse_aac_vbr_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.AAC_VBR);
        }

        [TestCase("Kirlian Camera - The Ice Curtain - Album 1998 - Ogg-Vorbis Q10", null, 0)]
        [TestCase("", "Vorbis Version 0 Audio", 500)]
        [TestCase("", "Opus Version 1 Audio", 501)]
        public void should_parse_vorbis_q10_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q10);
        }

        [TestCase("", "Vorbis Version 0 Audio", 320)]
        [TestCase("", "Opus Version 1 Audio", 321)]
        public void should_parse_vorbis_q9_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q9);
        }

        [TestCase("Various Artists - No New York [1978/Ogg/q8]", null, 0)]
        [TestCase("", "Vorbis Version 0 Audio", 256)]
        [TestCase("", "Opus Version 1 Audio", 257)]
        public void should_parse_vorbis_q8_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q8);
        }

        [TestCase("Masters_At_Work-Nuyorican_Soul-.Talkin_Loud.-1997-OGG.Q7", null, 0)]
        [TestCase("", "Vorbis Version 0 Audio", 224)]
        [TestCase("", "Opus Version 1 Audio", 225)]
        public void should_parse_vorbis_q7_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q7);
        }

        [TestCase("", "Vorbis Version 0 Audio", 192)]
        [TestCase("", "Opus Version 1 Audio", 193)]
        public void should_parse_vorbis_q6_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q6);
        }

        [TestCase("", "Vorbis Version 0 Audio", 160)]
        [TestCase("", "Opus Version 1 Audio", 161)]
        public void should_parse_vorbis_q5_quality(string title, string desc, int bitrate)
        {
            ParseAndVerifyQuality(title, desc, bitrate, Quality.VORBIS_Q5);
        }

        // Flack doesn't get match for 'FLAC' quality
        [TestCase("Roberta Flack 2006 - The Very Best of")]
        public void should_not_parse_flac_quality(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.Unknown);
        }

        [TestCase("The Chainsmokers & Coldplay - Something Just Like This")]
        [TestCase("Frank Ocean Blonde 2016")]
        //TODO: This should be parsed as Unknown and not MP3-96
        //[TestCase("A - NOW Thats What I Call Music 96 (2017) [Mp3~Kbps]")]
        [TestCase("Queen - The Ultimate Best Of Queen(2011)[mp3]")]
        [TestCase("Maroon 5 Ft Kendrick Lamar -Dont Wanna Know MP3 2016")]
        public void quality_parse(string title)
        {
            ParseAndVerifyQuality(title, null, 0, Quality.Unknown);
        }

        [Test, TestCaseSource(nameof(SelfQualityParserCases))]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = string.Format("Some album [{0}]", quality.Name);
            var result = QualityParser.ParseQuality(fileName, null, 0);
            result.Quality.Should().Be(quality);
        }

        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT")]
        public void should_parse_quality_from_name(string title)
        {
            QualityParser.ParseQuality(title, null, 0).QualityDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("01. Kanye West - Ultralight Beam.mp3")]
        [TestCase("01. Kanye West - Ultralight Beam.ogg")]
        //These get detected by name as we are looking for the extensions as identifiers for release names
        //[TestCase("01. Kanye West - Ultralight Beam.m4a")] 
        //[TestCase("01. Kanye West - Ultralight Beam.wma")]
        //[TestCase("01. Kanye West - Ultralight Beam.wav")]
        public void should_parse_quality_from_extension(string title)
        {
            QualityParser.ParseQuality(title, null, 0).QualityDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [Test]
        public void should_parse_null_quality_description_as_unknown()
        {
            QualityParser.ParseCodec(null, null).Should().Be(Codec.Unknown);
        }

        [TestCase("Artist Title - Album Title 2017 REPACK FLAC aAF", true)]
        [TestCase("Artist Title - Album Title 2017 RERIP FLAC aAF", true)]
        [TestCase("Artist Title - Album Title 2017 PROPER FLAC aAF", false)]
        public void should_be_able_to_parse_repack(string title, bool isRepack)
        {
            var result = QualityParser.ParseQuality(title, null, 0);
            result.Revision.Version.Should().Be(2);
            result.Revision.IsRepack.Should().Be(isRepack);
        }

        private void ParseAndVerifyQuality(string name, string desc, int bitrate, Quality quality, int sampleSize = 0)
        {
            var result = QualityParser.ParseQuality(name, desc, bitrate, sampleSize);
            result.Quality.Should().Be(quality);
        }

    }
}
