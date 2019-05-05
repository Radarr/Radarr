using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser
{
    public class QualityParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(QualityParser));

        private static readonly Regex ProperRegex = new Regex(@"\b(?<proper>proper)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RepackRegex = new Regex(@"\b(?<repack>repack|rerip)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new Regex(@"\dv(?<version>\d)\b|\[v(?<version>\d)\]",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RealRegex = new Regex(@"\b(?<real>REAL)\b",
                                                                RegexOptions.Compiled);

        private static readonly Regex BitRateRegex = new Regex(@"\b(?:(?<B096>96[ ]?kbps|96|[\[\(].*96.*[\]\)])|
                                                                (?<B128>128[ ]?kbps|128|[\[\(].*128.*[\]\)])|
                                                                (?<B160>160[ ]?kbps|160|[\[\(].*160.*[\]\)]|q5)|
                                                                (?<B192>192[ ]?kbps|192|[\[\(].*192.*[\]\)]|q6)|
                                                                (?<B224>224[ ]?kbps|224|[\[\(].*224.*[\]\)]|q7)|
                                                                (?<B256>256[ ]?kbps|256|itunes\splus|[\[\(].*256.*[\]\)]|q8)|
                                                                (?<B320>320[ ]?kbps|320|[\[\(].*320.*[\]\)]|q9)|
                                                                (?<B500>500[ ]?kbps|500|[\[\(].*500.*[\]\)]|q10)|
                                                                (?<VBRV0>V0[ ]?kbps|V0|[\[\(].*V0.*[\]\)])|
                                                                (?<VBRV2>V2[ ]?kbps|V2|[\[\(].*V2.*[\]\)]))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex SampleSizeRegex = new Regex(@"\b(?:(?<S24>24[ ]bit|24bit|[\[\(].*24bit.*[\]\)]))");

        private static readonly Regex CodecRegex = new Regex(@"\b(?:(?<MP1>MPEG Version \d(.5)? Audio, Layer 1|MP1)|(?<MP2>MPEG Version \d(.5)? Audio, Layer 2|MP2)|(?<MP3VBR>MP3.*VBR|MPEG Version \d(.5)? Audio, Layer 3 vbr)|(?<MP3CBR>MP3|MPEG Version \d(.5)? Audio, Layer 3)|(?<FLAC>flac)|(?<WAVPACK>wavpack|wv)|(?<ALAC>alac)|(?<WMA>WMA\d?)|(?<WAV>WAV|PCM)|(?<AAC>M4A|M4P|M4B|AAC|mp4a|MPEG-4 Audio(?!.*alac))|(?<OGG>OGG|OGA|Vorbis))\b|(?<APE>monkey's audio|[\[|\(].*\bape\b.*[\]|\)])|(?<OPUS>Opus Version \d(.5)? Audio|[\[|\(].*\bopus\b.*[\]|\)])",
                                                                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QualityModel ParseQuality(string name, string desc, int fileBitrate, int fileSampleSize = 0)
        {
            Logger.Debug("Trying to parse quality for {0}", name);

            var normalizedName = name.Replace('_', ' ').Trim().ToLower();
            var result = ParseQualityModifiers(name, normalizedName);

            if (desc.IsNotNullOrWhiteSpace())
            {
                var descCodec = ParseCodec(desc, "");
                Logger.Trace($"Got codec {descCodec}");

                result.Quality = FindQuality(descCodec, fileBitrate, fileSampleSize);

                if (result.Quality != Quality.Unknown)
                {
                    result.QualityDetectionSource = QualityDetectionSource.TagLib;
                    return result;
                }
            }

            var codec = ParseCodec(normalizedName,name);
            var bitrate = ParseBitRate(normalizedName);
            var sampleSize = ParseSampleSize(normalizedName);

            switch(codec)
            {
                case Codec.MP1:
                case Codec.MP2:
                    result.Quality = Quality.Unknown;
                    break;
                case Codec.MP3VBR:
                    if (bitrate == BitRate.VBRV0) { result.Quality = Quality.MP3_VBR; }
                    else if (bitrate == BitRate.VBRV2) { result.Quality = Quality.MP3_VBR_V2; }
                    else { result.Quality = Quality.Unknown; }
                    break;
                case Codec.MP3CBR:
                    if (bitrate == BitRate.B096) { result.Quality = Quality.MP3_096; }
                    else if (bitrate == BitRate.B128) { result.Quality = Quality.MP3_128; }
                    else if (bitrate == BitRate.B160) { result.Quality = Quality.MP3_160; }
                    else if (bitrate == BitRate.B192) { result.Quality = Quality.MP3_192; }
                    else if (bitrate == BitRate.B256) { result.Quality = Quality.MP3_256; }
                    else if (bitrate == BitRate.B320) { result.Quality = Quality.MP3_320; }
                    else { result.Quality = Quality.Unknown; }
                    break;
                case Codec.FLAC:
                    if (sampleSize == SampleSize.S24) {result.Quality = Quality.FLAC_24;}
                    else {result.Quality = Quality.FLAC;}
                    break;
                case Codec.ALAC:
                    result.Quality = Quality.ALAC;
                    break;
                case Codec.WAVPACK:
                    result.Quality = Quality.WAVPACK;
                    break;
                case Codec.APE:
                    result.Quality = Quality.APE;
                    break;
                case Codec.WMA:
                    result.Quality = Quality.WMA;
                    break;
                case Codec.WAV:
                    result.Quality = Quality.WAV;
                    break;
                case Codec.AAC:
                    if (bitrate == BitRate.B192) { result.Quality = Quality.AAC_192; }
                    else if (bitrate == BitRate.B256) { result.Quality = Quality.AAC_256; }
                    else if (bitrate == BitRate.B320) { result.Quality = Quality.AAC_320; }
                    else { result.Quality = Quality.AAC_VBR; }
                    break;
                case Codec.AACVBR:
                    result.Quality = Quality.AAC_VBR;
                    break;
                case Codec.OGG:
                case Codec.OPUS:
                    if (bitrate == BitRate.B160) { result.Quality = Quality.VORBIS_Q5; }
                    else if (bitrate == BitRate.B192) { result.Quality = Quality.VORBIS_Q6; }
                    else if (bitrate == BitRate.B224) { result.Quality = Quality.VORBIS_Q7; }
                    else if (bitrate == BitRate.B256) { result.Quality = Quality.VORBIS_Q8; }
                    else if (bitrate == BitRate.B320) { result.Quality = Quality.VORBIS_Q9; }
                    else if (bitrate == BitRate.B500) { result.Quality = Quality.VORBIS_Q10; }
                    else { result.Quality = Quality.Unknown; }
                    break;
                case Codec.Unknown:
                    if (bitrate == BitRate.B192) { result.Quality = Quality.MP3_192; }
                    else if (bitrate == BitRate.B256) { result.Quality = Quality.MP3_256; }
                    else if (bitrate == BitRate.B320) { result.Quality = Quality.MP3_320; }
                    else if (bitrate == BitRate.VBR) { result.Quality = Quality.MP3_VBR_V2; }
                    else { result.Quality = Quality.Unknown; }
                    break;
                default:
                    result.Quality = Quality.Unknown;
                    break;
            }

            //Based on extension
            if (result.Quality == Quality.Unknown && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(Path.GetExtension(name));
                    result.QualityDetectionSource = QualityDetectionSource.Extension;
                }
                catch (ArgumentException)
                {
                    //Swallow exception for cases where string contains illegal
                    //path characters.
                }
            }

            return result;
        }

        public static Codec ParseCodec(string name, string origName)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return Codec.Unknown;
            }

            var match = CodecRegex.Match(name);

            if (!match.Success) { return Codec.Unknown; }
            if (match.Groups["FLAC"].Success) { return Codec.FLAC; }
            if (match.Groups["ALAC"].Success) { return Codec.ALAC; }
            if (match.Groups["WMA"].Success) { return Codec.WMA; }
            if (match.Groups["WAV"].Success) { return Codec.WAV; }
            if (match.Groups["AAC"].Success) { return Codec.AAC; }
            if (match.Groups["OGG"].Success) { return Codec.OGG; }
            if (match.Groups["OPUS"].Success) { return Codec.OPUS; }
            if (match.Groups["MP1"].Success) { return Codec.MP1; }
            if (match.Groups["MP2"].Success) { return Codec.MP2; }
            if (match.Groups["MP3VBR"].Success) { return Codec.MP3VBR; }
            if (match.Groups["MP3CBR"].Success) { return Codec.MP3CBR; }
            if (match.Groups["WAVPACK"].Success) { return Codec.WAVPACK; }
            if (match.Groups["APE"].Success) { return Codec.APE; }

            return Codec.Unknown;
        }

        private static BitRate ParseBitRate(string name)
        {
            //var nameWithNoSpaces = Regex.Replace(name, @"\s+", "");
            var match = BitRateRegex.Match(name);

            if (!match.Success) return BitRate.Unknown;
            if (match.Groups["B096"].Success) { return BitRate.B096; }
            if (match.Groups["B128"].Success) { return BitRate.B128; }
            if (match.Groups["B160"].Success) { return BitRate.B160; }
            if (match.Groups["B192"].Success) { return BitRate.B192; }
            if (match.Groups["B224"].Success) { return BitRate.B224; }
            if (match.Groups["B256"].Success) { return BitRate.B256; }
            if (match.Groups["B320"].Success) { return BitRate.B320; }
            if (match.Groups["B500"].Success) { return BitRate.B500; }
            if (match.Groups["VBR"].Success)  { return BitRate.VBR; }
            if (match.Groups["VBRV0"].Success) { return BitRate.VBRV0; }
            if (match.Groups["VBRV2"].Success) { return BitRate.VBRV2; }

            return BitRate.Unknown;
        }

        private static SampleSize ParseSampleSize(string name)
        {
            var match = SampleSizeRegex.Match(name);

            if (!match.Success) { return SampleSize.Unknown; }
            if (match.Groups["S24"].Success) { return SampleSize.S24; }

            return SampleSize.Unknown;
        }

        private static Quality FindQuality(Codec codec, int bitrate, int sampleSize = 0)
        {
            switch (codec)
            {
                case Codec.MP1:
                case Codec.MP2:
                    return Quality.Unknown;
                case Codec.MP3VBR:
                    return Quality.MP3_VBR;
                case Codec.MP3CBR:
                    if (bitrate == 8) { return Quality.MP3_008; }
                    if (bitrate == 16) { return Quality.MP3_016; }
                    if (bitrate == 24) { return Quality.MP3_024; }
                    if (bitrate == 32) { return Quality.MP3_032; }
                    if (bitrate == 40) { return Quality.MP3_040; }
                    if (bitrate == 48) { return Quality.MP3_048; }
                    if (bitrate == 56) { return Quality.MP3_056; }
                    if (bitrate == 64) { return Quality.MP3_064; }
                    if (bitrate == 80) { return Quality.MP3_080; }
                    if (bitrate == 96) { return Quality.MP3_096; }
                    if (bitrate == 112) { return Quality.MP3_112; }
                    if (bitrate == 128) { return Quality.MP3_128; }
                    if (bitrate == 160) { return Quality.MP3_160; }
                    if (bitrate == 192) { return Quality.MP3_192; }
                    if (bitrate == 224) { return Quality.MP3_224; }
                    if (bitrate == 256) { return Quality.MP3_256; }
                    if (bitrate == 320) { return Quality.MP3_320; }
                    return Quality.Unknown;
                case Codec.FLAC:
                    if (sampleSize == 24) {return Quality.FLAC_24;}
                    return Quality.FLAC;
                case Codec.ALAC:
                    return Quality.ALAC;
                case Codec.WAVPACK:
                    return Quality.WAVPACK;
                case Codec.APE:
                    return Quality.APE;
                case Codec.WMA:
                    return Quality.WMA;
                case Codec.WAV:
                    return Quality.WAV;
                case Codec.AAC:
                    if (bitrate == 192) { return Quality.AAC_192; }
                    if (bitrate == 256) { return Quality.AAC_256; }
                    if (bitrate == 320) { return Quality.AAC_320; }
                    return Quality.AAC_VBR;
                case Codec.OGG:
                    if (bitrate == 160) { return Quality.VORBIS_Q5; }
                    if (bitrate == 192) { return Quality.VORBIS_Q6; }
                    if (bitrate == 224) { return Quality.VORBIS_Q7; }
                    if (bitrate == 256) { return Quality.VORBIS_Q8; }
                    if (bitrate == 320) { return Quality.VORBIS_Q9; }
                    if (bitrate == 500) { return Quality.VORBIS_Q10; }
                    return  Quality.Unknown;
                case Codec.OPUS:
                    if (bitrate < 130) { return Quality.Unknown; }
                    if (bitrate < 180) { return Quality.VORBIS_Q5; }
                    if (bitrate < 205) { return Quality.VORBIS_Q6; }
                    if (bitrate < 240) { return Quality.VORBIS_Q7; }
                    if (bitrate < 290) { return Quality.VORBIS_Q8; }
                    if (bitrate < 410) { return Quality.VORBIS_Q9; }
                    return Quality.VORBIS_Q10;
                default:
                    return Quality.Unknown;
            }
        }

        private static QualityModel ParseQualityModifiers(string name, string normalizedName)
        {
            var result = new QualityModel { Quality = Quality.Unknown };

            if (ProperRegex.IsMatch(normalizedName))
            {
                result.Revision.Version = 2;
            }

            if (RepackRegex.IsMatch(normalizedName))
            {
                result.Revision.Version = 2;
                result.Revision.IsRepack = true;
            }

            Match versionRegexResult = VersionRegex.Match(normalizedName);

            if (versionRegexResult.Success)
            {
                result.Revision.Version = Convert.ToInt32(versionRegexResult.Groups["version"].Value);
            }

            //TODO: re-enable this when we have a reliable way to determine real
            MatchCollection realRegexResult = RealRegex.Matches(name);

            if (realRegexResult.Count > 0)
            {
                result.Revision.Real = realRegexResult.Count;
            }

            return result;
        }
    }

    public enum Codec
    {
        MP1,
        MP2,
        MP3CBR,
        MP3VBR,
        FLAC,
        ALAC,
        APE,
        WAVPACK,
        WMA,
        AAC,
        AACVBR,
        OGG,
        OPUS,
        WAV,
        Unknown
    }

    public enum BitRate
    {
        B096,
        B128,
        B160,
        B192,
        B224,
        B256,
        B320,
        B500,
        VBR,
        VBRV0,
        VBRV2,
        Unknown,
    }

    public enum SampleSize
    {
        S24,
        Unknown
    }
}
