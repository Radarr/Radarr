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

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_|^)(?<italian>\b(?:ita|italian)\b)|(?<german>german\b|videomann)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:\W|_)(?:FR|VOSTFR|VO|VFF|VFQ|VFI|VF2|TRUEFRENCH)(?:\W|_))|(?<russian>\brus\b)|(?<dutch>nl\W?subs?)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<hebrew>\bHebDub\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)",
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

            if (lowerTitle.Contains("spanish"))
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

            if (lowerTitle.Contains("nordic"))
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

            var match = LanguageRegex.Match(title);

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
                languages.Add(Language.English);
            }

            return languages.DistinctBy(l => (int)l).ToList();
        }

        public static Language ParseSubtitleLanguage(string fileName)
        {
            try
            {
#if !LIBRARY
                Logger.Debug("Parsing language from subtitle file: {0}", fileName);
#endif

                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var languageMatch = SubtitleLanguageRegex.Match(simpleFilename);

                if (languageMatch.Success)
                {
                    var isoCode = languageMatch.Groups["iso_code"].Value;
                    var isoLanguage = IsoLanguages.Find(isoCode);

                    return isoLanguage?.Language ?? Language.Unknown;
                }
#if !LIBRARY
                Logger.Debug("Unable to parse langauge from subtitle file: {0}", fileName);
#endif
            }
            catch (Exception)
            {
#if !LIBRARY
                Logger.Debug("Failed parsing langauge from subtitle file: {0}", fileName);
#endif
            }

            return Language.Unknown;
        }
    }
}
