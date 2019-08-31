using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser
{
    public class QualityParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(QualityParser));

        private static readonly Regex SourceRegex = new Regex(@"\b(?:
                                                                (?<bluray>M?BluRay|Blu-Ray|HDDVD|BD(?!$)|BDISO|BD25|BD50|BR.?DISK)|
                                                                (?<webdl>WEB[-_. ]DL|HDRIP|WEBDL|WebRip|Web-Rip|iTunesHD|MaxdomeHD|NetflixU?HD|WebHD|WEBMux|[. ]WEB[. ](?:[xh]26[45]|DDP?5[. ]1)|\d+0p[-. ]WEB[-. ]|WEB-DLMux|\b\s\/\sWEB\s\/\s\b)|
                                                                (?<hdtv>HDTV)|
                                                                (?<bdrip>BDRip)|(?<brrip>BRRip)|
                                                                (?<dvdr>DVD-R|DVDR)|
                                                                (?<dvd>DVD|DVDRip|NTSC|PAL|xvidvd)|
                                                                (?<dsr>WS[-_. ]DSR|DSR)|
                                                                (?<regional>R[0-9]{1}|REGIONAL)|
                                                                (?<scr>SCR|SCREENER|DVDSCR|DVDSCREENER)|
                                                                (?<ts>TS|TELESYNC|HD-TS|HDTS|PDVD|TSRip|HDTSRip)|
                                                                (?<tc>TC|TELECINE|HD-TC|HDTC)|
                                                                (?<cam>CAMRIP|CAM|HDCAM|HD-CAM)|
                                                                (?<wp>WORKPRINT|WP)|
                                                                (?<pdtv>PDTV)|
                                                                (?<sdtv>SDTV)|
                                                                (?<tvrip>TVRip)
                                                                )\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex HardcodedSubsRegex = new Regex(@"\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex RemuxRegex = new Regex(@"\b(?<remux>(BD)?Remux)\b",
                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex BRDISKRegex = new Regex(@"\b(COMPLETE|ISO|BDISO|BD25|BD50|BR.?DISK)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ProperRegex = new Regex(@"\b(?<proper>proper)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RepackRegex = new Regex(@"\b(?<repack>repack|rerip)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new Regex(@"\dv(?<version>\d)\b|\[v(?<version>\d)\]",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RealRegex = new Regex(@"\b(?<real>REAL)\b",
                                                                RegexOptions.Compiled);

        private static readonly Regex ResolutionRegex = new Regex(@"\b(?:(?<R480p>480(i|p)|640x480|848x480)|(?<R576p>576(i|p))|(?<R720p>720(i|p)|1280x720)|(?<R1080p>1080(i|p)|1920x1080)|(?<R2160p>2160(i|p)|UHD))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CodecRegex = new Regex(@"\b(?:(?<x264>x264)|(?<h264>h264)|(?<xvidhd>XvidHD)|(?<xvid>X-?vid)|(?<divx>divx))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OtherSourceRegex = new Regex(@"(?<hdtv>HD[-_. ]TV)|(?<sdtv>SD[-_. ]TV)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AnimeBlurayRegex = new Regex(@"bd(?:720|1080)|(?<=[-_. (\[])bd(?=[-_. )\]])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HighDefPdtvRegex = new Regex(@"hr[-_. ]ws", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HDShitQualityRegex = new Regex(@"(HD-TS|HDTS|HDTSRip|HD-TC|HDTC|HDCAM|HD-CAM)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RawHDRegex = new Regex(@"\b(?<rawhd>RawHD|1080i[-_. ]HDTV|Raw[-_. ]HD|MPEG[-_. ]?2)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse quality for {0}", name);

            name = name.Trim();
            var normalizedName = name.Replace('_', ' ').Trim().ToLower();
            var result = ParseQualityModifiers(name, normalizedName);
            var subMatch = HardcodedSubsRegex.Matches(normalizedName).OfType<Match>().LastOrDefault();

            if (subMatch != null && subMatch.Success)
            {
                if (subMatch.Groups["hcsub"].Success)
                {
                    result.HardcodedSubs = subMatch.Groups["hcsub"].Value;
                }
                else if (subMatch.Groups["hc"].Success)
                {
                    result.HardcodedSubs = "Generic Hardcoded Subs";
                }
            }

            var sourceMatch = SourceRegex.Matches(normalizedName).OfType<Match>().LastOrDefault();
            var resolution = ParseResolution(normalizedName);
            var codecRegex = CodecRegex.Match(normalizedName);

            result.Resolution = resolution;

            if (BRDISKRegex.IsMatch(normalizedName) && sourceMatch?.Groups["bluray"].Success == true)
            {
                result.Modifier = Modifier.BRDISK;
                result.Source = Source.BLURAY;
            }

            if (RemuxRegex.IsMatch(normalizedName) && sourceMatch?.Groups["webdl"].Success != true && sourceMatch?.Groups["hdtv"].Success != true)
            {
                result.Modifier = Modifier.REMUX;
                result.Source = Source.BLURAY;
                return result; //We found remux!
            }
            
            if (RawHDRegex.IsMatch(normalizedName) && result.Modifier != Modifier.BRDISK)
            {
                result.Modifier = Modifier.RAWHD;
                result.Source = Source.TV;
                return result;
            }

            if (sourceMatch != null && sourceMatch.Success)
            {
                if (sourceMatch.Groups["bluray"].Success)
                {
                    result.Source = Source.BLURAY;

                    if (codecRegex.Groups["xvid"].Success || codecRegex.Groups["divx"].Success)
                    {
                        result.Resolution = Resolution.R480P;
                        result.Source = Source.DVD;
                        return result;
                    }

                    if (resolution == Resolution.Unknown) result.Resolution = Resolution.R720P; //Blurays are always at least 720p
                    if (resolution == Resolution.Unknown && result.Modifier == Modifier.BRDISK) result.Resolution = Resolution.R1080P; // BRDISKS are 1080p

                    return result;
                }

                if (sourceMatch.Groups["webdl"].Success)
                {
                    result.Source = Source.WEBDL;
                    if (resolution == Resolution.Unknown) result.Resolution = Resolution.R480P;
                    if (resolution == Resolution.Unknown && name.Contains("[WEBDL]")) result.Resolution = Resolution.R720P;
                    return result;
                }

                if (sourceMatch.Groups["hdtv"].Success)
                {
                    result.Source = Source.TV;
                    if (resolution == Resolution.Unknown) result.Resolution = Resolution.R480P; //hdtvs are always at least 480p (they might have been downscaled
                    if (resolution == Resolution.Unknown && name.Contains("[HDTV]")) result.Resolution = Resolution.R720P;
                    return result;
                }

                if (sourceMatch.Groups["bdrip"].Success ||
                    sourceMatch.Groups["brrip"].Success)
                {
                    if (codecRegex.Groups["xvid"].Success || codecRegex.Groups["divx"].Success)
                    {
                        // Since it's a dvd, res is 480p
                        result.Resolution = Resolution.R480P;
                        result.Source = Source.DVD;
                        return result;
                    }

                    if (resolution == Resolution.Unknown) result.Resolution = Resolution.R480P; //BDRip are always 480p or more.

                    result.Source = Source.BLURAY;
                    return result;
                }

                if (sourceMatch.Groups["wp"].Success)
                {
                    result.Source = Source.WORKPRINT;
                    return result;
                }

                if (sourceMatch.Groups["dvd"].Success)
                {
                    result.Resolution = Resolution.R480P;
                    result.Source = Source.DVD;
                    return result;
                }

                if (sourceMatch.Groups["dvdr"].Success)
                {
                    result.Resolution = Resolution.R480P;
                    result.Source = Source.DVD;
                    //result.Modifier = Modifier.REGIONAL;
                    return result;
                }

                if (sourceMatch.Groups["scr"].Success)
                {
                    result.Resolution = Resolution.R480P;
                    result.Source = Source.DVD;
                    result.Modifier = Modifier.SCREENER;
                    return result;
                }

                if (sourceMatch.Groups["regional"].Success)
                {
                    result.Resolution = Resolution.R480P;
                    result.Source = Source.DVD;
                    result.Modifier = Modifier.REGIONAL;
                    return result;
                }

                // they're shit, but at least 720p
                if (HDShitQualityRegex.IsMatch(normalizedName)) result.Resolution = Resolution.R720P;

                if (sourceMatch.Groups["cam"].Success)
                {
                    result.Source = Source.CAM;
                    return result;
                }

                if (sourceMatch.Groups["ts"].Success)
                {
                    result.Source = Source.TELESYNC;
                    return result;
                }

                if (sourceMatch.Groups["tc"].Success)
                {
                    result.Source = Source.TELECINE;
                    return result;
                }

                if (sourceMatch.Groups["pdtv"].Success ||
                    sourceMatch.Groups["sdtv"].Success ||
                    sourceMatch.Groups["dsr"].Success ||
                    sourceMatch.Groups["tvrip"].Success)
                {
                    result.Source = Source.TV;
                    if (HighDefPdtvRegex.IsMatch(normalizedName))
                    {
                        result.Resolution = Resolution.R720P;
                        return result;
                    }

                    result.Resolution = Resolution.R480P;
                    return result;
                }
            }

            //Anime Bluray matching
            if (AnimeBlurayRegex.Match(normalizedName).Success)
            {
                if (resolution == Resolution.R480P || resolution == Resolution.R576P || normalizedName.Contains("480p"))
                {
                    result.Resolution = Resolution.R480P;
                    result.Source = Source.DVD;
                    return result;
                }

                if (resolution == Resolution.R1080P || normalizedName.Contains("1080p"))
                {
                    result.Resolution = Resolution.R1080P;
                    result.Source = Source.BLURAY;
                    return result;
                }

                result.Resolution = Resolution.R720P;
                result.Source = Source.BLURAY;
                return result;
            }

            var otherSourceMatch = OtherSourceMatch(normalizedName);

            if (otherSourceMatch.Source != Source.UNKNOWN)
            {
                result.Source = otherSourceMatch.Source;
                result.Resolution = resolution == Resolution.Unknown ? otherSourceMatch.Resolution : resolution;
                return result;
            }

            if (resolution == Resolution.R2160P || resolution == Resolution.R1080P || resolution == Resolution.R720P)
            {
                result.Source = Source.WEBDL;
                return result;
            }

            if (resolution == Resolution.R480P)
            {
                result.Source = Source.DVD;
                return result;
            }

            if (codecRegex.Groups["x264"].Success)
            {
                result.Source = Source.DVD;
                result.Resolution = Resolution.R480P;
                return result;
            }

            if (normalizedName.Contains("848x480"))
            {

                result.Source = Source.DVD;
                result.Resolution = Resolution.R480P;
                return result;

            }

            if (normalizedName.Contains("1280x720"))
            {
                result.Resolution = Resolution.R720P;
                result.Source = Source.WEBDL;
                if (normalizedName.Contains("bluray"))
                {
                    result.Source = Source.BLURAY;
                }
                return result;
            }

            if (normalizedName.Contains("1920x1080"))
            {
                result.Resolution = Resolution.R1080P;
                result.Source = Source.WEBDL;
                if (normalizedName.Contains("bluray"))
                {
                    result.Source = Source.BLURAY;
                }
                return result;
            }

            if (normalizedName.Contains("bluray720p"))
            {
                result.Resolution = Resolution.R720P;
                result.Source = Source.BLURAY;
                return result;
            }

            if (normalizedName.Contains("bluray1080p"))
            {
                result.Resolution = Resolution.R1080P;
                result.Source = Source.BLURAY;
                return result;
            }

            //Based on extension
            if (result.Source == Source.UNKNOWN && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Source = MediaFileExtensions.GetSourceForExtension(Path.GetExtension(name));
                    result.Resolution = MediaFileExtensions.GetResolutionForExtension(Path.GetExtension(name));

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

        private static Resolution ParseResolution(string name)
        {
            var match = ResolutionRegex.Match(name);

            if (!match.Success) return Resolution.Unknown;
            if (match.Groups["R480p"].Success) return Resolution.R480P;
            if (match.Groups["R576p"].Success) return Resolution.R576P;
            if (match.Groups["R720p"].Success) return Resolution.R720P;
            if (match.Groups["R1080p"].Success) return Resolution.R1080P;
            if (match.Groups["R2160p"].Success) return Resolution.R2160P;

            return Resolution.Unknown;
        }

        private static QualityModel OtherSourceMatch(string name)
        {
            var match = OtherSourceRegex.Match(name);

            if (!match.Success) return new QualityModel();
            if (match.Groups["sdtv"].Success) return new QualityModel {Source = Source.TV, Resolution = Resolution.R480P};
            if (match.Groups["hdtv"].Success) return new QualityModel {Source = Source.TV, Resolution = Resolution.R720P};

            return new QualityModel();
        }

        private static QualityModel ParseQualityModifiers(string name, string normalizedName)
        {
            var result = new QualityModel();

            if (ProperRegex.IsMatch(normalizedName))
            {
                result.Revision.Version = 2;
            }

            if (RepackRegex.IsMatch(normalizedName))
            {
                result.Revision.Version = 2;
                result.Revision.IsRepack = true;
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
}
