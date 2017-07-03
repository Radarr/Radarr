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

        private static readonly Regex SourceRegex = new Regex(@"\b(?:
                                                                (?<bluray>BluRay|Blu-Ray|HDDVD|BD)|
                                                                (?<webdl>WEB[-_. ]DL|WEBDL|WebRip|iTunesHD|WebHD|[. ]WEB[. ](?:[xh]26[45]|DD5[. ]1)|\d+0p[. ]WEB[. ])|
                                                                (?<hdtv>HDTV)|
                                                                (?<bdrip>BDRip)|
                                                                (?<brrip>BRRip)|
                                                                (?<dvd>DVD|DVDRip|NTSC|PAL|xvidvd)|
                                                                (?<dsr>WS[-_. ]DSR|DSR)|
                                                                (?<pdtv>PDTV)|
                                                                (?<sdtv>SDTV)|
                                                                (?<tvrip>TVRip)
                                                                )\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex RawHDRegex = new Regex(@"\b(?<rawhd>RawHD|1080i[-_. ]HDTV|Raw[-_. ]HD|MPEG[-_. ]?2)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ProperRegex = new Regex(@"\b(?<proper>proper|repack|rerip)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new Regex(@"\dv(?<version>\d)\b|\[v(?<version>\d)\]",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RealRegex = new Regex(@"\b(?<real>REAL)\b",
                                                                RegexOptions.Compiled);

        private static readonly Regex BitRateRegex = new Regex(@"\b(?:(?<B192>192[ ]?kbps|192|[\[\(].*192.*[\]\)])|
                                                                (?<B256>256[ ]?kbps|256|[\[\(].*256.*[\]\)])|
                                                                (?<B320>320[ ]?kbps|320|[\[\(].*320.*[\]\)])|
                                                                (?<B512>512[ ]?kbps|512|[\[\(].*512.*[\]\)])|
                                                                (?<VBR>VBR[ ]?kbps|VBR|[\[\(].*VBR.*[\]\)])|
                                                                (?<FLAC>FLAC))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex CodecRegex = new Regex(@"\b(?:
                                                                  (?<MP3>MPEG Version \d+ Audio, Layer 3)|
                                                                  (?<FLAC>flac)|
                                                                  (?<VBR>VBR|MPEG Version \d+ Audio, Layer 3 VBR$)
                                                                  )\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);


        public static QualityModel ParseQuality(string desc, int bitrate, int sampleRate)
        {
            var result = new QualityModel { Quality = Quality.Unknown };

            switch (bitrate)
            {
                case 192:
                    result.Quality = Quality.MP3_192;
                    break;
                case 256:
                    result.Quality = Quality.MP3_256;
                    break;
                case 320:
                    result.Quality = Quality.MP3_320;
                    break;
                default:
                    var match = CodecRegex.Match(desc); //TODO: BUG: Figure out why this always fails
                    if (!match.Success) result.Quality = Quality.Unknown;
                    if (match.Groups["VBR"].Success) result.Quality =  Quality.MP3_VBR;
                    if (match.Groups["FLAC"].Success) result.Quality =  Quality.FLAC;
                    break;
            }


            return result;
        }

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse quality for {0}", name);

            var normalizedName = name.Replace('_', ' ').Trim().ToLower();
            var result = ParseQualityModifiers(name, normalizedName);
            var bitrate = ParseBitRate(normalizedName);

            switch(bitrate)
            {
                case BitRate.B192:
                    result.Quality = Quality.MP3_192;
                    break;
                case BitRate.B256:
                    result.Quality = Quality.MP3_256;
                    break;
                case BitRate.B320:
                    result.Quality = Quality.MP3_320;
                    break;
                case BitRate.B512:
                    result.Quality = Quality.MP3_512;
                    break;
                case BitRate.FLAC:
                    result.Quality = Quality.FLAC;
                    break;
                case BitRate.VBR:
                    result.Quality = Quality.MP3_VBR;
                    break;
            }

            //Based on extension
            if (result.Quality == Quality.Unknown && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(Path.GetExtension(name));
                    result.QualitySource = QualitySource.Extension;
                }
                catch (ArgumentException)
                {
                    //Swallow exception for cases where string contains illegal
                    //path characters.
                }
            }

            return result;
        }

        private static BitRate ParseCodec(string name)
        {
            var match = BitRateRegex.Match(name);

            if (!match.Success) return BitRate.Unknown;
            if (match.Groups["FLAC"].Success) return BitRate.FLAC;
            if (match.Groups["VBR"].Success) return BitRate.VBR;

            return BitRate.Unknown;
        }

        private static BitRate ParseBitRate(string name)
        {
            //var nameWithNoSpaces = Regex.Replace(name, @"\s+", "");
            var match = BitRateRegex.Match(name);

            if (!match.Success) return BitRate.Unknown;
            if (match.Groups["B192"].Success) return BitRate.B192;
            if (match.Groups["B256"].Success) return BitRate.B256;
            if (match.Groups["B320"].Success) return BitRate.B320;
            if (match.Groups["B512"].Success) return BitRate.B512;
            if (match.Groups["FLAC"].Success) return BitRate.FLAC;
            if (match.Groups["VBR"].Success) return BitRate.VBR;

            return BitRate.Unknown;
        }

        private static QualityModel ParseQualityModifiers(string name, string normalizedName)
        {
            var result = new QualityModel { Quality = Quality.Unknown };

            if (ProperRegex.IsMatch(normalizedName))
            {
                result.Revision.Version = 2;
            }

            var versionRegexResult = VersionRegex.Match(normalizedName);

            if (versionRegexResult.Success)
            {
                result.Revision.Version = Convert.ToInt32(versionRegexResult.Groups["version"].Value);
            }

            //TODO: re-enable this when we have a reliable way to determine real
            //TODO: Only treat it as a real if it comes AFTER the season/epsiode number
            var realRegexResult = RealRegex.Matches(name);

            if (realRegexResult.Count > 0)
            {
                result.Revision.Real = realRegexResult.Count;
            }

            return result;
        }
    }

    public enum BitRate
    {
        B192,
        B256,
        B320,
        B512,
        VBR,
        FLAC,
        Unknown,
    }
}
