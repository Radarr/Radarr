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
    public static class BookQualityParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(BookQualityParser));
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

        private static readonly Regex FormatRegex = new Regex(
            @"\b(?:
            (?<epub>EPUB|ePub)|
            (?<mobi>MOBI|Kindle)|
            (?<azw3>AZW3?|Kindle[-_. ]?Format[-_. ]?8|KF8)|
            (?<pdf>PDF)|
            (?<txt>TXT|Plain[-_. ]?Text)|
            (?<cbr>CBR|Comic[-_. ]?Book[-_. ]?RAR)|
            (?<cbz>CBZ|Comic[-_. ]?Book[-_. ]?ZIP)
        )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            RegexTimeout);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse book quality for '{0}'", name.SanitizeForLog());

            if (name.IsNullOrWhiteSpace())
            {
                return new QualityModel { Quality = Quality.EbookUnknown };
            }

            name = name.Trim();
            var normalizedName = name.Replace('_', ' ').Trim();

            var result = ParseQualityName(normalizedName);

            if (result.Quality == Quality.EbookUnknown && !name.ContainsInvalidPathChars())
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

                if (MediaFileExtensions.EbookExtensions.Contains(extension))
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
            var result = new QualityModel { Quality = Quality.EbookUnknown };

            var formatMatch = FormatRegex.Match(name);

            if (!formatMatch.Success)
            {
                return result;
            }

            result.SourceDetectionSource = QualityDetectionSource.Name;

            if (formatMatch.Groups["epub"].Success)
            {
                result.Quality = Quality.EPUB;
                return result;
            }

            if (formatMatch.Groups["mobi"].Success)
            {
                result.Quality = Quality.MOBI;
                return result;
            }

            if (formatMatch.Groups["azw3"].Success)
            {
                result.Quality = Quality.AZW3;
                return result;
            }

            if (formatMatch.Groups["pdf"].Success)
            {
                result.Quality = Quality.PDF;
                return result;
            }

            if (formatMatch.Groups["txt"].Success)
            {
                result.Quality = Quality.TXT;
                return result;
            }

            if (formatMatch.Groups["cbr"].Success || formatMatch.Groups["cbz"].Success)
            {
                result.Quality = Quality.EbookUnknown;
                return result;
            }

            return result;
        }

        public static bool IsBookFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var extension = Path.GetExtension(path);
                return MediaFileExtensions.EbookExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }
    }
}
