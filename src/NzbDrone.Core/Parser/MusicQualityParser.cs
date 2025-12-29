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
    public static class MusicQualityParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MusicQualityParser));
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

        private static readonly Regex BitDepthSampleRateRegex = new Regex(
            @"\b(?:
            (?<b24_192>24[-_/]?(?:bit[-_/]?)?192(?:khz?)?|192[-_/]?24)|
            (?<b24_176>24[-_/]?(?:bit[-_/]?)?176(?:\.4)?(?:khz?)?|176[-_/]?24)|
            (?<b24_96>24[-_/]?(?:bit[-_/]?)?96(?:khz?)?|96[-_/]?24)|
            (?<b24_88>24[-_/]?(?:bit[-_/]?)?88(?:\.2)?(?:khz?)?|88[-_/]?24)|
            (?<b24_48>24[-_/]?(?:bit[-_/]?)?48(?:khz?)?|48[-_/]?24)|
            (?<b24_44>24[-_/]?(?:bit[-_/]?)?44(?:\.1)?(?:khz?)?|44[-_/]?24)|
            (?<b16_48>16[-_/]?(?:bit[-_/]?)?48(?:khz?)?|48[-_/]?16)|
            (?<b16_44>16[-_/]?(?:bit[-_/]?)?44(?:\.1)?(?:khz?)?|44[-_/]?16|Redbook|CD[-_]?Quality)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex HiResRegex = new Regex(
            @"\b(?:
            (?<hires>Hi[-_]?Res|HiRes|High[-_]?Resolution|Hi[-_]?Resolution|Studio[-_]?Master)|
            (?<b24>24[-_]?bit)|
            (?<b16>16[-_]?bit)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex DsdRegex = new Regex(
            @"\b(?:
            (?<dsd512>DSD[-_]?512|DSD22\.?5|22\.?5[-_]?MHz)|
            (?<dsd256>DSD[-_]?256|DSD11\.?2|11\.?2[-_]?MHz|Quad[-_]?DSD)|
            (?<dsd128>DSD[-_]?128|DSD5\.?6|5\.?6[-_]?MHz|Double[-_]?DSD)|
            (?<dsd64>DSD[-_]?64|DSD2\.?8|2\.?8[-_]?MHz|SACD|DSF|DFF)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex FormatRegex = new Regex(
            @"\b(?:
            (?<flac>FLAC)|
            (?<alac>ALAC|Apple[-_]?Lossless)|
            (?<wav>WAV|PCM|LPCM)|
            (?<aiff>AIFF?)|
            (?<ape>APE|Monkey'?s?[-_]?Audio)|
            (?<wavpack>WavPack|WV)|
            (?<mqa_studio>MQA[-_]?Studio)|
            (?<mqa>MQA)|
            (?<mp3>MP3)|
            (?<aac>AAC|M4A)|
            (?<ogg>OGG|Vorbis)|
            (?<opus>Opus)|
            (?<wma>WMA)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex BitrateRegex = new Regex(
            @"\b(?:
            (?<b320>(?:MP3|AAC|OGG)?[-_]?320(?:[-_]?k(?:bps)?)?|V0|CBR[-_]?320)|
            (?<b256>(?:MP3|AAC|OGG)?[-_]?256(?:[-_]?k(?:bps)?)?)|
            (?<b192>(?:MP3|AAC|OGG)?[-_]?192(?:[-_]?k(?:bps)?)?)|
            (?<b128>(?:MP3|AAC|OGG)?[-_]?128(?:[-_]?k(?:bps)?)?)|
            (?<vbr>VBR|V[0-2])
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex LosslessRegex = new Regex(
            @"\b(?:Lossless|Perfect[-_]?Quality)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        private static readonly Regex SourceRegex = new Regex(
            @"\b(?:
            (?<web>WEB|iTunes|Spotify|Tidal|Qobuz|Deezer|Amazon[-_]?Music|Apple[-_]?Music|Bandcamp|HDTracks)|
            (?<cd>CD[-_]?Rip|CD|CDDA)|
            (?<vinyl>Vinyl[-_]?Rip|Vinyl|LP[-_]?Rip)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse music quality for '{0}'", name.SanitizeForLog());

            if (name.IsNullOrWhiteSpace())
            {
                return new QualityModel { Quality = Quality.MusicUnknown };
            }

            name = name.Trim();
            var normalizedName = name.Replace('_', ' ').Trim();

            var result = ParseQualityName(normalizedName);

            if (result.Quality == Quality.MusicUnknown && !name.ContainsInvalidPathChars())
            {
                result = ParseFromExtension(name, result);
            }

            return result;
        }

        private static QualityModel ParseFromExtension(string name, QualityModel result)
        {
            try
            {
                var extension = Path.GetExtension(name);
                if (string.IsNullOrEmpty(extension))
                {
                    return result;
                }

                if (MediaFileExtensions.MusicExtensions.Contains(extension))
                {
                    result.Quality = GetDefaultQualityForExtension(extension);
                    result.SourceDetectionSource = QualityDetectionSource.Extension;
                }
            }
            catch (ArgumentException ex)
            {
                Logger.Debug(ex, "Unable to parse extension from '{0}'", name.SanitizeForLog());
            }

            return result;
        }

        private static Quality GetDefaultQualityForExtension(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".flac":
                    return Quality.MusicFLAC_16_44;
                case ".wav":
                    return Quality.MusicWAV_16_44;
                case ".aiff":
                case ".aif":
                    return Quality.MusicAIFF_16_44;
                case ".dsf":
                case ".dff":
                    return Quality.MusicDSD64;
                case ".alac":
                case ".m4a":
                    return Quality.MusicALAC_16_44;
                case ".ape":
                    return Quality.MusicAPE;
                case ".wv":
                    return Quality.MusicWavPack;
                case ".mp3":
                    return Quality.MusicMP3_320;
                case ".aac":
                    return Quality.MusicAAC_256;
                case ".ogg":
                case ".oga":
                    return Quality.MusicOGG_320;
                case ".opus":
                    return Quality.MusicOpus_192;
                case ".wma":
                    return Quality.MusicWMA;
                default:
                    return Quality.MusicUnknown;
            }
        }

        private static QualityModel ParseQualityName(string name)
        {
            var result = new QualityModel { Quality = Quality.MusicUnknown };

            var dsdMatch = DsdRegex.Match(name);
            if (dsdMatch.Success)
            {
                return CreateResult(ParseDsdQuality(dsdMatch));
            }

            var formatMatch = FormatRegex.Match(name);
            var bitDepthSampleRateMatch = BitDepthSampleRateRegex.Match(name);
            var hiResMatch = HiResRegex.Match(name);
            var bitrateMatch = BitrateRegex.Match(name);

            var quality = TryParseFromFormat(formatMatch, bitDepthSampleRateMatch, hiResMatch, bitrateMatch);
            if (quality != null)
            {
                return CreateResult(quality);
            }

            quality = TryParseFromBitDepthOrLossless(name, bitDepthSampleRateMatch, hiResMatch);
            if (quality != null)
            {
                return CreateResult(quality);
            }

            return result;
        }

        private static QualityModel CreateResult(Quality quality)
        {
            return new QualityModel
            {
                Quality = quality,
                SourceDetectionSource = QualityDetectionSource.Name
            };
        }

        private static Quality TryParseFromFormat(Match formatMatch, Match bitDepthSampleRateMatch, Match hiResMatch, Match bitrateMatch)
        {
            if (formatMatch.Groups["mqa_studio"].Success)
            {
                return Quality.MusicMQA_Studio;
            }

            if (formatMatch.Groups["mqa"].Success)
            {
                return Quality.MusicMQA;
            }

            if (formatMatch.Groups["flac"].Success)
            {
                return DetermineFlacQuality(bitDepthSampleRateMatch, hiResMatch);
            }

            if (formatMatch.Groups["wav"].Success)
            {
                return DetermineWavQuality(bitDepthSampleRateMatch, hiResMatch);
            }

            if (formatMatch.Groups["aiff"].Success)
            {
                return DetermineAiffQuality(bitDepthSampleRateMatch, hiResMatch);
            }

            if (formatMatch.Groups["alac"].Success)
            {
                return DetermineAlacQuality(bitDepthSampleRateMatch, hiResMatch);
            }

            if (formatMatch.Groups["ape"].Success)
            {
                return Quality.MusicAPE;
            }

            if (formatMatch.Groups["wavpack"].Success)
            {
                return Quality.MusicWavPack;
            }

            if (formatMatch.Groups["mp3"].Success || bitrateMatch.Success)
            {
                return DetermineMp3Quality(bitrateMatch);
            }

            if (formatMatch.Groups["aac"].Success)
            {
                return DetermineAacQuality(bitrateMatch);
            }

            if (formatMatch.Groups["ogg"].Success)
            {
                return DetermineOggQuality(bitrateMatch);
            }

            if (formatMatch.Groups["opus"].Success)
            {
                return DetermineOpusQuality(bitrateMatch);
            }

            if (formatMatch.Groups["wma"].Success)
            {
                return Quality.MusicWMA;
            }

            return null;
        }

        private static Quality TryParseFromBitDepthOrLossless(string name, Match bitDepthSampleRateMatch, Match hiResMatch)
        {
            if (bitDepthSampleRateMatch.Success || hiResMatch.Success)
            {
                return DetermineFlacQuality(bitDepthSampleRateMatch, hiResMatch);
            }

            if (LosslessRegex.IsMatch(name))
            {
                return Quality.MusicFLAC_16_44;
            }

            return null;
        }

        private static Quality ParseDsdQuality(Match match)
        {
            if (match.Groups["dsd512"].Success)
            {
                return Quality.MusicDSD512;
            }

            if (match.Groups["dsd256"].Success)
            {
                return Quality.MusicDSD256;
            }

            if (match.Groups["dsd128"].Success)
            {
                return Quality.MusicDSD128;
            }

            return Quality.MusicDSD64;
        }

        private static (int bitDepth, int sampleRate) ExtractBitDepthSampleRate(Match bitDepthSampleRate, Match hiRes)
        {
            if (bitDepthSampleRate.Groups["b24_192"].Success)
            {
                return (24, 192);
            }

            if (bitDepthSampleRate.Groups["b24_176"].Success)
            {
                return (24, 176);
            }

            if (bitDepthSampleRate.Groups["b24_96"].Success)
            {
                return (24, 96);
            }

            if (bitDepthSampleRate.Groups["b24_88"].Success)
            {
                return (24, 88);
            }

            if (bitDepthSampleRate.Groups["b24_48"].Success)
            {
                return (24, 48);
            }

            if (bitDepthSampleRate.Groups["b24_44"].Success)
            {
                return (24, 44);
            }

            if (bitDepthSampleRate.Groups["b16_48"].Success)
            {
                return (16, 48);
            }

            if (bitDepthSampleRate.Groups["b16_44"].Success)
            {
                return (16, 44);
            }

            if (hiRes.Groups["hires"].Success || hiRes.Groups["b24"].Success)
            {
                return (24, 96);
            }

            return (16, 44);
        }

        private static Quality DetermineFlacQuality(Match bitDepthSampleRate, Match hiRes)
        {
            var (bitDepth, sampleRate) = ExtractBitDepthSampleRate(bitDepthSampleRate, hiRes);

            return (bitDepth, sampleRate) switch
            {
                (24, 192) => Quality.MusicFLAC_24_192,
                (24, 176) => Quality.MusicFLAC_24_176,
                (24, 96) => Quality.MusicFLAC_24_96,
                (24, 88) => Quality.MusicFLAC_24_88,
                (24, 48) => Quality.MusicFLAC_24_48,
                (24, 44) => Quality.MusicFLAC_24_44,
                (16, 48) => Quality.MusicFLAC_16_48,
                _ => Quality.MusicFLAC_16_44
            };
        }

        private static Quality DetermineWavQuality(Match bitDepthSampleRate, Match hiRes)
        {
            var (bitDepth, sampleRate) = ExtractBitDepthSampleRate(bitDepthSampleRate, hiRes);

            return (bitDepth, sampleRate) switch
            {
                (24, 192) => Quality.MusicWAV_24_192,
                (24, 176) => Quality.MusicWAV_24_176,
                (24, 96) => Quality.MusicWAV_24_96,
                (24, 88) => Quality.MusicWAV_24_88,
                (24, 48) => Quality.MusicWAV_24_48,
                (24, 44) => Quality.MusicWAV_24_44,
                (16, 48) => Quality.MusicWAV_16_48,
                _ => Quality.MusicWAV_16_44
            };
        }

        private static Quality DetermineAiffQuality(Match bitDepthSampleRate, Match hiRes)
        {
            var (bitDepth, sampleRate) = ExtractBitDepthSampleRate(bitDepthSampleRate, hiRes);

            return (bitDepth, sampleRate) switch
            {
                (24, 192) => Quality.MusicAIFF_24_192,
                (24, 176) => Quality.MusicAIFF_24_176,
                (24, 96) => Quality.MusicAIFF_24_96,
                (24, 88) => Quality.MusicAIFF_24_88,
                (24, 48) => Quality.MusicAIFF_24_48,
                (24, 44) => Quality.MusicAIFF_24_44,
                (16, 48) => Quality.MusicAIFF_16_48,
                _ => Quality.MusicAIFF_16_44
            };
        }

        private static Quality DetermineAlacQuality(Match bitDepthSampleRate, Match hiRes)
        {
            var (bitDepth, sampleRate) = ExtractBitDepthSampleRate(bitDepthSampleRate, hiRes);

            return (bitDepth, sampleRate) switch
            {
                (24, 192) => Quality.MusicALAC_24_192,
                (24, 96) => Quality.MusicALAC_24_96,
                (24, 88) => Quality.MusicALAC_24_96,
                (24, 48) => Quality.MusicALAC_24_48,
                (24, 44) => Quality.MusicALAC_24_48,
                (24, 176) => Quality.MusicALAC_24_48,
                (16, 48) => Quality.MusicALAC_16_48,
                _ => Quality.MusicALAC_16_44
            };
        }

        private static Quality DetermineMp3Quality(Match bitrateMatch)
        {
            if (bitrateMatch.Groups["b320"].Success || bitrateMatch.Groups["vbr"].Success)
            {
                return Quality.MusicMP3_320;
            }

            if (bitrateMatch.Groups["b256"].Success)
            {
                return Quality.MusicMP3_256;
            }

            if (bitrateMatch.Groups["b192"].Success)
            {
                return Quality.MusicMP3_192;
            }

            if (bitrateMatch.Groups["b128"].Success)
            {
                return Quality.MusicMP3_128;
            }

            return Quality.MusicMP3_320;
        }

        private static Quality DetermineAacQuality(Match bitrateMatch)
        {
            if (bitrateMatch.Groups["b320"].Success)
            {
                return Quality.MusicAAC_320;
            }

            if (bitrateMatch.Groups["b256"].Success)
            {
                return Quality.MusicAAC_256;
            }

            return Quality.MusicAAC_128;
        }

        private static Quality DetermineOggQuality(Match bitrateMatch)
        {
            if (bitrateMatch.Groups["b320"].Success)
            {
                return Quality.MusicOGG_320;
            }

            if (bitrateMatch.Groups["b256"].Success)
            {
                return Quality.MusicOGG_256;
            }

            if (bitrateMatch.Groups["b192"].Success)
            {
                return Quality.MusicOGG_192;
            }

            return Quality.MusicOGG_128;
        }

        private static Quality DetermineOpusQuality(Match bitrateMatch)
        {
            if (bitrateMatch.Groups["b256"].Success || bitrateMatch.Groups["b320"].Success)
            {
                return Quality.MusicOpus_256;
            }

            if (bitrateMatch.Groups["b192"].Success)
            {
                return Quality.MusicOpus_192;
            }

            return Quality.MusicOpus_128;
        }

        public static bool IsMusicFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var extension = Path.GetExtension(path);
                return MediaFileExtensions.MusicExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }
    }
}
