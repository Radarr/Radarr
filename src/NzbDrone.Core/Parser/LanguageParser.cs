using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public static class LanguageParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LanguageParser));

        private static readonly Dictionary<string, Language> KeywordToLanguage = new Dictionary<string, Language>
        {
            { "english", Language.English },
            { "spanish", Language.Spanish },
            { "danish", Language.Danish },
            { "dutch", Language.Dutch },
            { "japanese", Language.Japanese },
            { "icelandic", Language.Icelandic },
            { "mandarin", Language.Chinese },
            { "cantonese", Language.Chinese },
            { "chinese", Language.Chinese },
            { "korean", Language.Korean },
            { "russian", Language.Russian },
            { "romanian", Language.Romanian },
            { "hindi", Language.Hindi },
            { "arabic", Language.Arabic },
            { "thai", Language.Thai },
            { "bulgarian", Language.Bulgarian },
            { "polish", Language.Polish },
            { "vietnamese", Language.Vietnamese },
            { "swedish", Language.Swedish },
            { "norwegian", Language.Norwegian },
            { "finnish", Language.Finnish },
            { "turkish", Language.Turkish },
            { "portuguese", Language.Portuguese },
            { "brazilian", Language.PortugueseBR },
            { "hungarian", Language.Hungarian },
            { "hebrew", Language.Hebrew },
            { "ukrainian", Language.Ukrainian },
            { "persian", Language.Persian },
            { "bengali", Language.Bengali },
            { "slovak", Language.Slovak },
            { "latvian", Language.Latvian },
            { "latino", Language.SpanishLatino },
            { "tamil", Language.Tamil },
            { "telugu", Language.Telugu },
            { "malayalam", Language.Malayalam },
            { "kannada", Language.Kannada },
            { "albanian", Language.Albanian },
            { "afrikaans", Language.Afrikaans },
            { "marathi", Language.Marathi },
            { "tagalog", Language.Tagalog }
        };

        private static readonly Dictionary<string, Language> CaseSensitiveGroupToLanguage = new Dictionary<string, Language>
        {
            { "english", Language.English },
            { "lithuanian", Language.Lithuanian },
            { "czech", Language.Czech },
            { "polish", Language.Polish },
            { "bulgarian", Language.Bulgarian },
            { "slovak", Language.Slovak },
            { "spanish", Language.Spanish },
            { "german", Language.German }
        };

        private static readonly Dictionary<string, Language> CaseInsensitiveGroupToLanguage = new Dictionary<string, Language>
        {
            { "english", Language.English },
            { "italian", Language.Italian },
            { "german", Language.German },
            { "flemish", Language.Flemish },
            { "greek", Language.Greek },
            { "french", Language.French },
            { "russian", Language.Russian },
            { "bulgarian", Language.Bulgarian },
            { "brazilian", Language.PortugueseBR },
            { "dutch", Language.Dutch },
            { "hungarian", Language.Hungarian },
            { "hebrew", Language.Hebrew },
            { "polish", Language.Polish },
            { "chinese", Language.Chinese },
            { "spanish", Language.Spanish },
            { "catalan", Language.Catalan },
            { "ukrainian", Language.Ukrainian },
            { "latvian", Language.Latvian },
            { "romanian", Language.Romanian },
            { "telugu", Language.Telugu },
            { "vietnamese", Language.Vietnamese },
            { "japanese", Language.Japanese },
            { "korean", Language.Korean },
            { "urdu", Language.Urdu },
            { "romansh", Language.Romansh },
            { "mongolian", Language.Mongolian },
            { "georgian", Language.Georgian },
            { "original", Language.Original }
        };

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_|^)(?<english>\beng\b)|
                                                                            (?<italian>\b(?:ita|italian)\b)|
                                                                            (?<german>(?:swiss)?german\b|videomann|ger[. ]dub|\bger\b)|
                                                                            (?<flemish>flemish)|
                                                                            (?<bulgarian>bgaudio)|
                                                                            (?<romanian>rodubbed)|
                                                                            (?<brazilian>\b(dublado|pt-BR)\b)|
                                                                            (?<greek>greek)|
                                                                            (?<french>\b(?:FR|VO|VF|VFF|VFQ|VFI|VF2|TRUEFRENCH|FRENCH|FRE|FRA)\b)|
                                                                            (?<russian>\b(?:rus|ru)\b)|
                                                                            (?<hungarian>\b(?:HUNDUB|HUN)\b)|
                                                                            (?<hebrew>\b(?:HebDub|HebDubbed)\b)|
                                                                            (?<polish>\b(?:PL\W?DUB|DUB\W?PL|LEK\W?PL|PL\W?LEK)\b)|
                                                                            (?<chinese>\[(?:CH[ST]|BIG5|GB)\]|简|繁|字幕)|
                                                                            (?<ukrainian>(?:(?:\dx)?UKR))|
                                                                            (?<spanish>\b(?:español|castellano)\b)|
                                                                            (?<catalan>\b(?:catalan?|catalán|català)\b)|
                                                                            (?<latvian>\b(?:lat|lav|lv)\b)|
                                                                            (?<telugu>\btel\b)|
                                                                            (?<vietnamese>\bVIE\b)|
                                                                            (?<japanese>\bJAP\b)|
                                                                            (?<korean>\bKOR\b)|
                                                                            (?<urdu>\burdu\b)|
                                                                            (?<romansh>\b(?:romansh|rumantsch|romansch)\b)|
                                                                            (?<mongolian>\b(?:mongolian|khalkha)\b)|
                                                                            (?<georgian>\b(?:georgian|geo|ka|kat)\b)|
                                                                            (?<original>\b(?:orig|original)\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<english>\bEN\b)|
                                                                                                          (?<lithuanian>\bLT\b)|
                                                                                                          (?<czech>\bCZ\b)|
                                                                                                          (?<polish>\bPL\b)|
                                                                                                          (?<bulgarian>\bBG\b)|
                                                                                                          (?<slovak>\bSK\b)|
                                                                                                          (?<german>\bDE\b)|
                                                                                                          (?<spanish>\b(?<!DTS[._ -])ES\b))(?:(?i)(?![\W|_|^]SUB))",
                                                                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex GermanDualLanguageRegex = new (@"(?<!WEB[-_. ]?)\bDL\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex GermanMultiLanguageRegex = new (@"\bML\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?([-_. ](?<tags>forced|foreign|default|cc|psdh|sdh))*[-_. ](?<iso_code>[a-z]{2,3})([-_. ](?<tags>forced|foreign|default|cc|psdh|sdh))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleLanguageTitleRegex = new Regex(@".+?(\.((?<tags1>forced|foreign|default|cc|psdh|sdh)|(?<iso_code>[a-z]{2,3})))*[-_. ](?<title>[^.]*)(\.((?<tags2>forced|foreign|default|cc|psdh|sdh)|(?<iso_code>[a-z]{2,3})))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleTitleRegex = new Regex(@"^((?<title>.+) - )?(?<copy>(?<!\d+)\d{1,3}(?!\d+))$", RegexOptions.Compiled);

        public static List<Language> ParseLanguages(string title)
        {
            var lowerTitle = title.ToLower();
            var languages = new List<Language>();

            AddLanguagesFromKeywords(lowerTitle, languages);
            AddLanguagesFromCaseSensitiveRegex(title, languages);
            AddLanguagesFromCaseInsensitiveRegex(title, languages);

            if (!languages.Any())
            {
                languages.Add(Language.Unknown);
            }

            AddGermanMultiLanguageVariants(title, languages);

            return languages.DistinctBy(l => (int)l).ToList();
        }

        private static void AddLanguagesFromKeywords(string lowerTitle, List<Language> languages)
        {
            foreach (var mapping in KeywordToLanguage)
            {
                if (lowerTitle.Contains(mapping.Key))
                {
                    languages.Add(mapping.Value);
                }
            }
        }

        private static void AddLanguagesFromCaseSensitiveRegex(string title, List<Language> languages)
        {
            var matches = CaseSensitiveLanguageRegex.Matches(title);

            foreach (Match match in matches)
            {
                foreach (var mapping in CaseSensitiveGroupToLanguage)
                {
                    if (match.Groups[mapping.Key].Captures.Any())
                    {
                        languages.Add(mapping.Value);
                    }
                }
            }
        }

        private static void AddLanguagesFromCaseInsensitiveRegex(string title, List<Language> languages)
        {
            var matches = LanguageRegex.Matches(title);

            foreach (Match match in matches)
            {
                foreach (var mapping in CaseInsensitiveGroupToLanguage)
                {
                    if (match.Groups[mapping.Key].Success)
                    {
                        languages.Add(mapping.Value);
                    }
                }
            }
        }

        private static void AddGermanMultiLanguageVariants(string title, List<Language> languages)
        {
            if (languages.Count != 1 || languages.Single() != Language.German)
            {
                return;
            }

            if (GermanDualLanguageRegex.IsMatch(title))
            {
                Logger.Trace("Adding original language because the release title contains German DL tag");
                languages.Add(Language.Original);
            }
            else if (GermanMultiLanguageRegex.IsMatch(title))
            {
                Logger.Trace("Adding original language and English because the release title contains German ML tag");
                languages.Add(Language.Original);
                languages.Add(Language.English);
            }
        }

        public static List<string> ParseLanguageTags(string fileName)
        {
            try
            {
                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var match = SubtitleLanguageRegex.Match(simpleFilename);
                var languageTags = match.Groups["tags"].Captures
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

                foreach (var language in Language.All)
                {
                    if (simpleFilename.EndsWith(language.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return language;
                    }
                }

                Logger.Debug("Unable to parse language from subtitle file: {0}", fileName);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed parsing language from subtitle file: {0}", fileName);
            }

            return Language.Unknown;
        }

        public static SubtitleTitleInfo ParseBasicSubtitle(string fileName)
        {
            return new SubtitleTitleInfo
            {
                TitleFirst = false,
                LanguageTags = ParseLanguageTags(fileName),
                Language = ParseSubtitleLanguage(fileName)
            };
        }

        public static SubtitleTitleInfo ParseSubtitleLanguageInformation(string fileName)
        {
            var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
            var matchTitle = SubtitleLanguageTitleRegex.Match(simpleFilename);

            if (!matchTitle.Groups["title"].Success || (matchTitle.Groups["iso_code"].Captures.Count is var languageCodeNumber && languageCodeNumber != 1))
            {
                Logger.Debug("Could not parse a title from subtitle file: {0}. Falling back to parsing without title.", fileName);

                return ParseBasicSubtitle(fileName);
            }

            var isoCode = matchTitle.Groups["iso_code"].Value;
            var isoLanguage = IsoLanguages.Find(isoCode.ToLower());

            var language = isoLanguage?.Language ?? Language.Unknown;

            var languageTags = matchTitle.Groups["tags1"].Captures
                .Union(matchTitle.Groups["tags2"].Captures)
                .Cast<Capture>()
                .Where(tag => !tag.Value.Empty())
                .Select(tag => tag.Value.ToLower());
            var rawTitle = matchTitle.Groups["title"].Value;

            var subtitleTitleInfo = new SubtitleTitleInfo
            {
                TitleFirst = matchTitle.Groups["tags1"].Captures.Empty(),
                LanguageTags = languageTags.ToList(),
                RawTitle = rawTitle,
                Language = language
            };

            UpdateTitleAndCopyFromTitle(subtitleTitleInfo);

            return subtitleTitleInfo;
        }

        public static void UpdateTitleAndCopyFromTitle(SubtitleTitleInfo subtitleTitleInfo)
        {
            if (subtitleTitleInfo.RawTitle is null)
            {
                subtitleTitleInfo.Title = null;
                subtitleTitleInfo.Copy = 0;
            }
            else if (SubtitleTitleRegex.Match(subtitleTitleInfo.RawTitle) is var match && match.Success)
            {
                subtitleTitleInfo.Title = match.Groups["title"].Success ? match.Groups["title"].ToString() : null;
                subtitleTitleInfo.Copy = int.Parse(match.Groups["copy"].ToString());
            }
            else
            {
                subtitleTitleInfo.Title = subtitleTitleInfo.RawTitle;
                subtitleTitleInfo.Copy = 0;
            }
        }
    }
}
