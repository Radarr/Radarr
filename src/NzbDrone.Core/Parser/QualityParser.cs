using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
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

        private static readonly Regex CodecRegex = new Regex(@"\b(?:(?<PDF>PDF)|(?<MOBI>MOBI)|(?<EPUB>EPUB)|(?<AZW3>AZW3?)|(?<MP1>MPEG Version \d(.5)? Audio, Layer 1|MP1)|(?<MP2>MPEG Version \d(.5)? Audio, Layer 2|MP2)|(?<MP3VBR>MP3.*VBR|MPEG Version \d(.5)? Audio, Layer 3 vbr)|(?<MP3CBR>MP3|MPEG Version \d(.5)? Audio, Layer 3)|(?<FLAC>flac)|(?<WAVPACK>wavpack|wv)|(?<ALAC>alac)|(?<WMA>WMA\d?)|(?<WAV>WAV|PCM)|(?<AAC>M4A|M4P|M4B|AAC|mp4a|MPEG-4 Audio(?!.*alac))|(?<OGG>OGG|OGA|Vorbis))\b|(?<APE>monkey's audio|[\[|\(].*\bape\b.*[\]|\)])|(?<OPUS>Opus Version \d(.5)? Audio|[\[|\(].*\bopus\b.*[\]|\)])",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QualityModel ParseQuality(string name, string desc = null)
        {
            Logger.Debug("Trying to parse quality for {0}", name);

            var normalizedName = name.Replace('_', ' ').Trim().ToLower();
            var result = ParseQualityModifiers(name, normalizedName);

            if (desc.IsNotNullOrWhiteSpace())
            {
                var descCodec = ParseCodec(desc, "");
                Logger.Trace($"Got codec {descCodec}");

                result.Quality = FindQuality(descCodec);

                if (result.Quality != Quality.Unknown)
                {
                    result.QualityDetectionSource = QualityDetectionSource.TagLib;
                    return result;
                }
            }

            var codec = ParseCodec(normalizedName, name);

            switch (codec)
            {
                case Codec.PDF:
                    result.Quality = Quality.PDF;
                    break;
                case Codec.EPUB:
                    result.Quality = Quality.EPUB;
                    break;
                case Codec.MOBI:
                    result.Quality = Quality.MOBI;
                    break;
                case Codec.AZW3:
                    result.Quality = Quality.AZW3;
                    break;
                case Codec.FLAC:
                case Codec.ALAC:
                case Codec.WAVPACK:
                    result.Quality = Quality.FLAC;
                    break;
                case Codec.MP1:
                case Codec.MP2:
                case Codec.MP3VBR:
                case Codec.MP3CBR:
                case Codec.APE:
                case Codec.WMA:
                case Codec.WAV:
                case Codec.AAC:
                case Codec.AACVBR:
                case Codec.OGG:
                case Codec.OPUS:
                    result.Quality = Quality.MP3_320;
                    break;
                case Codec.Unknown:
                default:
                    result.Quality = Quality.Unknown;
                    break;
            }

            //Based on extension
            if (result.Quality == Quality.Unknown && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(name.GetPathExtension());
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

            if (!match.Success)
            {
                return Codec.Unknown;
            }

            if (match.Groups["PDF"].Success)
            {
                return Codec.PDF;
            }

            if (match.Groups["EPUB"].Success)
            {
                return Codec.EPUB;
            }

            if (match.Groups["MOBI"].Success)
            {
                return Codec.MOBI;
            }

            if (match.Groups["AZW3"].Success)
            {
                return Codec.AZW3;
            }

            if (match.Groups["FLAC"].Success)
            {
                return Codec.FLAC;
            }

            if (match.Groups["ALAC"].Success)
            {
                return Codec.ALAC;
            }

            if (match.Groups["WMA"].Success)
            {
                return Codec.WMA;
            }

            if (match.Groups["WAV"].Success)
            {
                return Codec.WAV;
            }

            if (match.Groups["AAC"].Success)
            {
                return Codec.AAC;
            }

            if (match.Groups["OGG"].Success)
            {
                return Codec.OGG;
            }

            if (match.Groups["OPUS"].Success)
            {
                return Codec.OPUS;
            }

            if (match.Groups["MP1"].Success)
            {
                return Codec.MP1;
            }

            if (match.Groups["MP2"].Success)
            {
                return Codec.MP2;
            }

            if (match.Groups["MP3VBR"].Success)
            {
                return Codec.MP3VBR;
            }

            if (match.Groups["MP3CBR"].Success)
            {
                return Codec.MP3CBR;
            }

            if (match.Groups["WAVPACK"].Success)
            {
                return Codec.WAVPACK;
            }

            if (match.Groups["APE"].Success)
            {
                return Codec.APE;
            }

            return Codec.Unknown;
        }

        private static Quality FindQuality(Codec codec)
        {
            switch (codec)
            {
                case Codec.ALAC:
                case Codec.FLAC:
                case Codec.WAVPACK:
                case Codec.WAV:
                    return Quality.FLAC;
                case Codec.MP1:
                case Codec.MP2:
                case Codec.MP3VBR:
                case Codec.MP3CBR:
                case Codec.APE:
                case Codec.WMA:
                case Codec.AAC:
                case Codec.OGG:
                case Codec.OPUS:
                default:
                    return Quality.MP3_320;
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
        PDF,
        EPUB,
        MOBI,
        AZW3,
        Unknown
    }
}
