using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    public static class LanguageParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LanguageParser));

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_|^)(?<italian>\b(?:ita|italian)\b)|(?<german>\b(?:german|videomann|ger)\b)|(?<flemish>flemish)|(?<bulgarian>bgaudio)|(?<brazilian>dublado)|(?<greek>greek)|(?<french>\b(?:FR|VO|VFF|VFQ|VFI|VF2|TRUEFRENCH|FRE|FRA)\b)|(?<russian>\brus\b)|(?<english>\beng\b)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<hebrew>\bHebDub\b)|(?<polish>\b(?:PL\W?DUB|DUB\W?PL|LEK\W?PL|PL\W?LEK)\b)|(?<chinese>\[(?:CH[ST]|BIG5|GB)\]|简|繁|字幕)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)|(?<polish>\bPL\b))(?:(?i)(?![\W|_|^]SUB))",
                                                                RegexOptions.Compiled);

        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?[-_. ](?<iso_code>[a-z]{2,3})(?:[-_. ]forced)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<Language> ParseLanguages(string title)
        {
            var lowerTitle = title.ToLower();
            var languages = new List<Language>();

            if (lowerTitle.Contains("english"))
            {
                languages.Add(Language.English);
            }

            if (lowerTitle.Contains("french"))
            {
                languages.Add(Language.French);
            }

            if (lowerTitle.Contains("spanish") || lowerTitle.Contains("castellano"))
            {
                languages.Add(Language.Spanish);
            }

            if (lowerTitle.Contains("danish"))
            {
                languages.Add(Language.Danish);
            }

            if (lowerTitle.Contains("dutch"))
            {
                languages.Add(Language.Dutch);
            }

            if (lowerTitle.Contains("japanese"))
            {
                languages.Add(Language.Japanese);
            }

            if (lowerTitle.Contains("icelandic"))
            {
                languages.Add(Language.Icelandic);
            }

            if (lowerTitle.Contains("mandarin") || lowerTitle.Contains("cantonese") || lowerTitle.Contains("chinese"))
            {
                languages.Add(Language.Chinese);
            }

            if (lowerTitle.Contains("korean"))
            {
                languages.Add(Language.Korean);
            }

            if (lowerTitle.Contains("russian"))
            {
                languages.Add(Language.Russian);
            }

            if (lowerTitle.Contains("romanian"))
            {
                languages.Add(Language.Romanian);
            }

            if (lowerTitle.Contains("hindi"))
            {
                languages.Add(Language.Hindi);
            }

            if (lowerTitle.Contains("arabic"))
            {
                languages.Add(Language.Arabic);
            }

            if (lowerTitle.Contains("thai"))
            {
                languages.Add(Language.Thai);
            }

            if (lowerTitle.Contains("bulgarian"))
            {
                languages.Add(Language.Bulgarian);
            }

            if (lowerTitle.Contains("polish"))
            {
                languages.Add(Language.Polish);
            }

            if (lowerTitle.Contains("vietnamese"))
            {
                languages.Add(Language.Vietnamese);
            }

            if (lowerTitle.Contains("swedish"))
            {
                languages.Add(Language.Swedish);
            }

            if (lowerTitle.Contains("norwegian"))
            {
                languages.Add(Language.Norwegian);
            }

            if (lowerTitle.Contains("finnish"))
            {
                languages.Add(Language.Finnish);
            }

            if (lowerTitle.Contains("turkish"))
            {
                languages.Add(Language.Turkish);
            }

            if (lowerTitle.Contains("portuguese"))
            {
                languages.Add(Language.Portuguese);
            }

            if (lowerTitle.Contains("hungarian"))
            {
                languages.Add(Language.Hungarian);
            }

            if (lowerTitle.Contains("hebrew"))
            {
                languages.Add(Language.Hebrew);
            }

            // Case sensitive
            var caseSensitiveMatch = CaseSensitiveLanguageRegex.Match(title);

            if (caseSensitiveMatch.Groups["lithuanian"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Lithuanian);
            }

            if (caseSensitiveMatch.Groups["czech"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Czech);
            }

            if (caseSensitiveMatch.Groups["polish"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Polish);
            }

            var matches = LanguageRegex.Matches(title);

            foreach (Match match in matches)
            {
                if (match.Groups["italian"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Italian);
                }

                if (match.Groups["german"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.German);
                }

                if (match.Groups["flemish"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Flemish);
                }

                if (match.Groups["greek"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Greek);
                }

                if (match.Groups["french"].Success)
                {
                    languages.Add(Language.French);
                }

                if (match.Groups["russian"].Success)
                {
                    languages.Add(Language.Russian);
                }

                if (match.Groups["english"].Success)
                {
                    languages.Add(Language.English);
                }

                if (match.Groups["bulgarian"].Success)
                {
                    languages.Add(Language.Bulgarian);
                }

                if (match.Groups["brazilian"].Success)
                {
                    languages.Add(Language.PortugueseBR);
                }

                if (match.Groups["dutch"].Success)
                {
                    languages.Add(Language.Dutch);
                }

                if (match.Groups["hungarian"].Success)
                {
                    languages.Add(Language.Hungarian);
                }

                if (match.Groups["hebrew"].Success)
                {
                    languages.Add(Language.Hebrew);
                }

                if (match.Groups["polish"].Success)
                {
                    languages.Add(Language.Polish);
                }

                if (match.Groups["chinese"].Success)
                {
                    languages.Add(Language.Chinese);
                }
            }

            if (title.ToLower().Contains("multi"))
            {
                //Let's add english language to multi release as a safe guard.
                if (!languages.Contains(Language.English) && languages.Count < 2)
                {
                    languages.Add(Language.English);
                }
            }

            if (!languages.Any())
            {
                languages.Add(Language.Unknown);
            }

            return languages.DistinctBy(l => (int)l).ToList();
        }

        public static Language ParseSubtitleLanguage(string fileName)
        {
            try
            {
                Logger.Debug("Parsing language from subtitle file: {0}", fileName);

                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var languageMatch = SubtitleLanguageRegex.Match(simpleFilename);

                if (languageMatch.Success)
                {
                    var isoCode = languageMatch.Groups["iso_code"].Value;
                    var isoLanguage = IsoLanguages.Find(isoCode);

                    return isoLanguage?.Language ?? Language.Unknown;
                }

                Logger.Debug("Unable to parse langauge from subtitle file: {0}", fileName);
            }
            catch (Exception)
            {
                Logger.Debug("Failed parsing langauge from subtitle file: {0}", fileName);
            }

            return Language.Unknown;
        }
    }
}
