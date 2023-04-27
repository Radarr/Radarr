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

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_|^)(?<italian>\b(?:ita|italian)\b)|
                                                                            (?<german>german\b|videomann|ger[. ]dub)|
                                                                            (?<flemish>flemish)|
                                                                            (?<bulgarian>bgaudio)|
                                                                            (?<romanian>rodubbed)|
                                                                            (?<brazilian>dublado)|
                                                                            (?<greek>greek)|
                                                                            (?<french>\b(?:FR|VO|VFF|VFQ|VFI|VF2|TRUEFRENCH|FRE|FRA)\b)|
                                                                            (?<russian>\brus\b)|
                                                                            (?<english>\beng\b)|
                                                                            (?<hungarian>\b(?:HUNDUB|HUN)\b)|
                                                                            (?<hebrew>\bHebDub\b)|
                                                                            (?<polish>\b(?:PL\W?DUB|DUB\W?PL|LEK\W?PL|PL\W?LEK)\b)|
                                                                            (?<chinese>\[(?:CH[ST]|BIG5|GB)\]|简|繁|字幕)|
                                                                            (?<ukrainian>(?:(?:\dx)?UKR))|
                                                                            (?<spanish>\b(?:español|castellano)\b)|
                                                                            (?<latvian>\bLV\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<lithuanian>\bLT\b)|
                                                                                                          (?<czech>\bCZ\b)|
                                                                                                          (?<polish>\bPL\b)|
                                                                                                          (?<bulgarian>\bBG\b))(?:(?i)(?![\W|_|^]SUB))|
                                                                                                          (?<slovak>\bSK\b)",
                                                                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?[-_. ](?<iso_code>[a-z]{2,3})([-_. ](?<tags>full|forced|foreign|default|cc|psdh|sdh))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            if (lowerTitle.Contains("brazilian"))
            {
                languages.Add(Language.PortugueseBR);
            }

            if (lowerTitle.Contains("hungarian"))
            {
                languages.Add(Language.Hungarian);
            }

            if (lowerTitle.Contains("hebrew"))
            {
                languages.Add(Language.Hebrew);
            }

            if (lowerTitle.Contains("ukrainian"))
            {
                languages.Add(Language.Ukrainian);
            }

            if (lowerTitle.Contains("persian"))
            {
                languages.Add(Language.Persian);
            }

            if (lowerTitle.Contains("bengali"))
            {
                languages.Add(Language.Bengali);
            }

            if (lowerTitle.Contains("slovak"))
            {
                languages.Add(Language.Slovak);
            }

            if (lowerTitle.Contains("latvian"))
            {
                languages.Add(Language.Latvian);
            }

            if (lowerTitle.Contains("latino"))
            {
                languages.Add(Language.SpanishLatino);
            }

            if (lowerTitle.Contains("catalan"))
            {
                languages.Add(Language.Catalan);
            }

            if (lowerTitle.Contains("tamil"))
            {
                languages.Add(Language.Tamil);
            }

            // Case sensitive
            var caseSensitiveMatchs = CaseSensitiveLanguageRegex.Matches(title);

            foreach (Match match in caseSensitiveMatchs)
            {
                if (match.Groups["lithuanian"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Lithuanian);
                }

                if (match.Groups["czech"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Czech);
                }

                if (match.Groups["polish"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Polish);
                }

                if (match.Groups["bulgarian"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Bulgarian);
                }

                if (match.Groups["slovak"].Captures.Cast<Capture>().Any())
                {
                    languages.Add(Language.Slovak);
                }
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

                if (match.Groups["spanish"].Success)
                {
                    languages.Add(Language.Spanish);
                }

                if (match.Groups["ukrainian"].Success)
                {
                    languages.Add(Language.Ukrainian);
                }

                if (match.Groups["latvian"].Success)
                {
                    languages.Add(Language.Latvian);
                }

                if (match.Groups["romanian"].Success)
                {
                    languages.Add(Language.Romanian);
                }
            }

            if (!languages.Any())
            {
                languages.Add(Language.Unknown);
            }

            return languages.DistinctBy(l => (int)l).ToList();
        }

        public static List<string> ParseLanguageTags(string fileName)
        {
            try
            {
                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var match = SubtitleLanguageRegex.Match(simpleFilename);
                var languageTags = match.Groups["tags"].Captures.Cast<Capture>()
                    .Where(tag => !tag.Value.Empty())
                    .Select(tag => tag.Value.ToLower());
                return languageTags.ToList();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed parsing language tags from subtitle file: {0}", fileName);
            }

            return new List<string>();
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
                    var isoLanguage = IsoLanguages.Find(isoCode.ToLower());

                    return isoLanguage?.Language ?? Language.Unknown;
                }

                foreach (Language language in Language.All)
                {
                    if (simpleFilename.EndsWith(language.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return language;
                    }
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
