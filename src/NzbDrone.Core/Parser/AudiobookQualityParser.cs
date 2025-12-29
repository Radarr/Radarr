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
    public static class AudiobookQualityParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(AudiobookQualityParser));
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

        private static readonly Regex BitrateRegex = new Regex(
            @"\b(?:
            (?<mp3_128>MP3[-_. ]?128|128[-_. ]?kbps?|128k(?:bit)?)|
            (?<mp3_320>MP3[-_. ]?320|320[-_. ]?kbps?|320k(?:bit)?|MP3[-_. ]?(?:V0|CBR)|V0)|
            (?<vbr>VBR|V[0-9])
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex FormatRegex = new Regex(
            @"\b(?:
            (?<m4b>M4B)|
            (?<mp3>MP3)|
            (?<flac>FLAC)|
            (?<aax>AAX|Audible[-_. ]?Enhanced)|
            (?<aa>AA(?!C)|Audible(?![-_. ]?Enhanced))
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        private static readonly Regex AudiobookIndicatorRegex = new Regex(
            @"\b(?:
            Audiobook|Audio[-_. ]?Book|Unabridged|Abridged|Narrated[-_. ]?by|Read[-_. ]?by
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse audiobook quality for '{0}'", name.SanitizeForLog());

            if (name.IsNullOrWhiteSpace())
            {
                return new QualityModel { Quality = Quality.AudiobookUnknown };
            }

            name = name.Trim();
            var normalizedName = name.Replace('_', ' ').Trim();

            var result = ParseQualityName(normalizedName);

            if (result.Quality == Quality.AudiobookUnknown && !name.ContainsInvalidPathChars())
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

                if (MediaFileExtensions.AudiobookExtensions.Contains(extension))
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(extension);
                    result.SourceDetectionSource = QualityDetectionSource.Extension;
                }
            }
            catch (ArgumentException ex)
            {
                Logger.Debug(ex, "Unable to parse extension from '{0}'", name.SanitizeForLog());
            }

            return result;
        }

        private static QualityModel ParseQualityName(string name)
        {
            var result = new QualityModel { Quality = Quality.AudiobookUnknown };

            var bitrateMatch = BitrateRegex.Match(name);
            var formatMatch = FormatRegex.Match(name);

            if (formatMatch.Success)
            {
                result.SourceDetectionSource = QualityDetectionSource.Name;
                var formatQuality = ParseFormatMatch(formatMatch, bitrateMatch);
                if (formatQuality != Quality.AudiobookUnknown)
                {
                    result.Quality = formatQuality;
                    return result;
                }
            }

            if (bitrateMatch.Success)
            {
                result.SourceDetectionSource = QualityDetectionSource.Name;
                var bitrateQuality = ParseBitrateMatch(bitrateMatch);
                if (bitrateQuality != Quality.AudiobookUnknown)
                {
                    result.Quality = bitrateQuality;
                    return result;
                }
            }

            return result;
        }

        private static Quality ParseFormatMatch(Match formatMatch, Match bitrateMatch)
        {
            if (formatMatch.Groups["m4b"].Success || formatMatch.Groups["aax"].Success)
            {
                return Quality.M4B;
            }

            if (formatMatch.Groups["aa"].Success)
            {
                return Quality.MP3_128;
            }

            if (formatMatch.Groups["flac"].Success)
            {
                return Quality.AudioFLAC;
            }

            if (formatMatch.Groups["mp3"].Success)
            {
                return bitrateMatch.Groups["mp3_128"].Success ? Quality.MP3_128 : Quality.MP3_320;
            }

            return Quality.AudiobookUnknown;
        }

        private static Quality ParseBitrateMatch(Match bitrateMatch)
        {
            if (bitrateMatch.Groups["mp3_128"].Success)
            {
                return Quality.MP3_128;
            }

            if (bitrateMatch.Groups["mp3_320"].Success || bitrateMatch.Groups["vbr"].Success)
            {
                return Quality.MP3_320;
            }

            return Quality.AudiobookUnknown;
        }

        public static bool IsAudiobookFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var extension = Path.GetExtension(path);
                return MediaFileExtensions.AudiobookExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }

        public static bool LooksLikeAudiobook(string name)
        {
            return AudiobookIndicatorRegex.IsMatch(name);
        }
    }
}
