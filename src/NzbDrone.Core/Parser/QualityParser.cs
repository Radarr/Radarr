﻿using System;
using System.IO;
using System.Linq;
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
                                                                (?<bluray>M?BluRay|Blu-Ray|HDDVD|BD(?!$)|BDISO|BD25|BD50|BR.?DISK)|
                                                                (?<webdl>WEB[-_. ]DL|WEBDL|AmazonHD|iTunesHD|MaxdomeHD|NetflixU?HD|WebHD|[. ]WEB[. ](?:[xh]26[45]|DDP?5[. ]1)|\d+0p[-. ]WEB[-. ]|WEB-DLMux|\b\s\/\sWEB\s\/\s\b)|
                                                                (?<webrip>WebRip|Web-Rip|WEBMux)|
                                                                (?<hdtv>HDTV)|
                                                                (?<bdrip>BDRip)|
                                                                (?<brrip>BRRip)|
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

        private static readonly Regex RawHDRegex = new Regex(@"\b(?<rawhd>RawHD|1080i[-_. ]HDTV|Raw[-_. ]HD|MPEG[-_. ]?2)\b",
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

        private static readonly Regex ResolutionRegex = new Regex(@"\b(?:(?<R480p>480p|640x480|848x480)|(?<R576p>576p)|(?<R720p>720p|1280x720)|(?<R1080p>1080p|1920x1080|1440p|FHD|1080i)|(?<R2160p>2160p|4k[-_. ](?:UHD|HEVC|BD)|(?:UHD|HEVC|BD)[-_. ]4k))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CodecRegex = new Regex(@"\b(?:(?<x264>x264)|(?<h264>h264)|(?<xvidhd>XvidHD)|(?<xvid>X-?vid)|(?<divx>divx))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OtherSourceRegex = new Regex(@"(?<hdtv>HD[-_. ]TV)|(?<sdtv>SD[-_. ]TV)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AnimeBlurayRegex = new Regex(@"bd(?:720|1080)|(?<=[-_. (\[])bd(?=[-_. )\]])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HighDefPdtvRegex = new Regex(@"hr[-_. ]ws", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RemuxRegex = new Regex(@"\b(?<remux>(BD)?[-_. ]?Remux)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HDShitQualityRegex = new Regex(@"(HD-TS|HDTS|HDTSRip|HD-TC|HDTC|HDCAM|HD-CAM)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HardcodedSubsRegex = new Regex(@"\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Debug("Trying to parse quality for {0}", name);

            name = name.Trim();

            var result = ParseQualityName(name);

            // Based on extension
            if (result.Quality == Quality.Unknown && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(Path.GetExtension(name));
                    result.QualityDetectionSource = QualityDetectionSource.Extension;
                }
                catch (ArgumentException)
                {
                    // Swallow exception for cases where string contains illegal
                    // path characters.
                }
            }

            return result;
        }

        public static QualityModel ParseQualityName(string name)
        {
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

            var test = SourceRegex.Matches(normalizedName);
            var sourceMatch = SourceRegex.Matches(normalizedName).OfType<Match>().LastOrDefault();
            var resolution = ParseResolution(normalizedName);
            var codecRegex = CodecRegex.Match(normalizedName);
            var remuxMatch = RemuxRegex.IsMatch(normalizedName);
            var brDiskMatch = BRDISKRegex.IsMatch(normalizedName);

            if (sourceMatch != null && sourceMatch.Success)
            {
                if (sourceMatch.Groups["bluray"].Success)
                {
                    if (brDiskMatch)
                    {
                        result.Quality = Quality.BRDISK;
                        return result;
                    }

                    if (codecRegex.Groups["xvid"].Success || codecRegex.Groups["divx"].Success)
                    {
                        result.Quality = Quality.Bluray480p;
                        return result;
                    }

                    if (resolution == Resolution.R2160p)
                    {
                        result.Quality = remuxMatch ? Quality.Remux2160p : Quality.Bluray2160p;

                        return result;
                    }

                    if (resolution == Resolution.R1080p)
                    {
                        result.Quality = remuxMatch ? Quality.Remux1080p : Quality.Bluray1080p;
                        return result;
                    }

                    if (resolution == Resolution.R576p)
                    {
                        result.Quality = Quality.Bluray576p;
                        return result;
                    }

                    if (resolution == Resolution.R480p)
                    {
                        result.Quality = Quality.Bluray480p;
                        return result;
                    }

                    // Treat a remux without a source as 1080p, not 720p.
                    if (remuxMatch)
                    {
                        result.Quality = Quality.Remux1080p;
                        return result;
                    }

                    result.Quality = Quality.Bluray720p;
                    return result;
                }

                if (sourceMatch.Groups["webdl"].Success)
                {
                    if (resolution == Resolution.R2160p)
                    {
                        result.Quality = Quality.WEBDL2160p;
                        return result;
                    }

                    if (resolution == Resolution.R1080p)
                    {
                        result.Quality = Quality.WEBDL1080p;
                        return result;
                    }

                    if (resolution == Resolution.R720p)
                    {
                        result.Quality = Quality.WEBDL720p;
                        return result;
                    }

                    if (name.Contains("[WEBDL]"))
                    {
                        result.Quality = Quality.WEBDL720p;
                        return result;
                    }

                    result.Quality = Quality.WEBDL480p;
                    return result;
                }

                if (sourceMatch.Groups["webrip"].Success)
                {
                    if (resolution == Resolution.R2160p)
                    {
                        result.Quality = Quality.WEBRip2160p;
                        return result;
                    }

                    if (resolution == Resolution.R1080p)
                    {
                        result.Quality = Quality.WEBRip1080p;
                        return result;
                    }

                    if (resolution == Resolution.R720p)
                    {
                        result.Quality = Quality.WEBRip720p;
                        return result;
                    }

                    result.Quality = Quality.WEBRip480p;
                    return result;
                }

                if (sourceMatch.Groups["scr"].Success)
                {
                    result.Quality = Quality.DVDSCR;
                    return result;
                }

                if (sourceMatch.Groups["cam"].Success)
                {
                    result.Quality = Quality.CAM;
                    return result;
                }

                if (sourceMatch.Groups["ts"].Success)
                {
                    result.Quality = Quality.TELESYNC;
                    result.Quality.Resolution = (int)resolution;
                    return result;
                }

                if (sourceMatch.Groups["tc"].Success)
                {
                    result.Quality = Quality.TELECINE;
                    return result;
                }

                if (sourceMatch.Groups["wp"].Success)
                {
                    result.Quality = Quality.WORKPRINT;
                    return result;
                }

                if (sourceMatch.Groups["regional"].Success)
                {
                    result.Quality = Quality.REGIONAL;
                    return result;
                }

                if (sourceMatch.Groups["hdtv"].Success)
                {
                    if (resolution == Resolution.R2160p)
                    {
                        result.Quality = Quality.HDTV2160p;
                        return result;
                    }

                    if (resolution == Resolution.R1080p)
                    {
                        result.Quality = Quality.HDTV1080p;
                        return result;
                    }

                    if (resolution == Resolution.R720p)
                    {
                        result.Quality = Quality.HDTV720p;
                        return result;
                    }

                    if (name.Contains("[HDTV]"))
                    {
                        result.Quality = Quality.HDTV720p;
                        return result;
                    }

                    result.Quality = Quality.SDTV;
                    return result;
                }

                if (sourceMatch.Groups["bdrip"].Success ||
                    sourceMatch.Groups["brrip"].Success)
                {
                    switch (resolution)
                    {
                        case Resolution.R720p:
                            result.Quality = Quality.Bluray720p;
                            return result;
                        case Resolution.R1080p:
                            result.Quality = Quality.Bluray1080p;
                            return result;
                        case Resolution.R576p:
                            result.Quality = Quality.Bluray576p;
                            return result;
                        default:
                            result.Quality = Quality.Bluray480p;
                            return result;
                    }
                }

                if (sourceMatch.Groups["dvd"].Success)
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                if (sourceMatch.Groups["pdtv"].Success ||
                    sourceMatch.Groups["sdtv"].Success ||
                    sourceMatch.Groups["dsr"].Success ||
                    sourceMatch.Groups["tvrip"].Success)
                {
                    if (resolution == Resolution.R1080p || normalizedName.Contains("1080p"))
                    {
                        result.Quality = Quality.HDTV1080p;
                        return result;
                    }

                    if (resolution == Resolution.R720p || normalizedName.Contains("720p"))
                    {
                        result.Quality = Quality.HDTV720p;
                        return result;
                    }

                    if (HighDefPdtvRegex.IsMatch(normalizedName))
                    {
                        result.Quality = Quality.HDTV720p;
                        return result;
                    }

                    result.Quality = Quality.SDTV;
                    return result;
                }
            }

            // Anime Bluray matching
            if (AnimeBlurayRegex.Match(normalizedName).Success)
            {
                if (resolution == Resolution.R480p || resolution == Resolution.R576p || normalizedName.Contains("480p"))
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                if (resolution == Resolution.R1080p || normalizedName.Contains("1080p"))
                {
                    result.Quality = remuxMatch ? Quality.Remux1080p : Quality.Bluray1080p;
                    return result;
                }

                if (resolution == Resolution.R2160p || normalizedName.Contains("2160p"))
                {
                    result.Quality = remuxMatch ? Quality.Remux2160p : Quality.Bluray2160p;
                    return result;
                }

                // Treat a remux without a source as 1080p, not 720p.
                if (remuxMatch)
                {
                    result.Quality = Quality.Bluray1080p;
                    return result;
                }

                result.Quality = Quality.Bluray720p;
                return result;
            }

            if (resolution == Resolution.R2160p)
            {
                result.Quality = remuxMatch ? Quality.Remux2160p : Quality.HDTV2160p;
                return result;
            }

            if (resolution == Resolution.R1080p)
            {
                result.Quality = remuxMatch ? Quality.Remux1080p : Quality.HDTV1080p;
                return result;
            }

            if (resolution == Resolution.R720p)
            {
                result.Quality = Quality.HDTV720p;
                return result;
            }

            if (resolution == Resolution.R480p)
            {
                result.Quality = Quality.SDTV;
                return result;
            }

            if (codecRegex.Groups["x264"].Success)
            {
                result.Quality = Quality.SDTV;
                return result;
            }

            if (normalizedName.Contains("848x480"))
            {
                if (normalizedName.Contains("dvd"))
                {
                    result.Quality = Quality.DVD;
                }

                result.Quality = Quality.SDTV;
            }

            if (normalizedName.Contains("1280x720"))
            {
                if (normalizedName.Contains("bluray"))
                {
                    result.Quality = Quality.Bluray720p;
                }

                result.Quality = Quality.HDTV720p;
            }

            if (normalizedName.Contains("1920x1080"))
            {
                if (normalizedName.Contains("bluray"))
                {
                    result.Quality = Quality.Bluray1080p;
                }

                result.Quality = Quality.HDTV1080p;
            }

            if (normalizedName.Contains("bluray720p"))
            {
                result.Quality = Quality.Bluray720p;
            }

            if (normalizedName.Contains("bluray1080p"))
            {
                result.Quality = Quality.Bluray1080p;
            }

            if (normalizedName.Contains("bluray2160p"))
            {
                result.Quality = Quality.Bluray2160p;
            }

            var otherSourceMatch = OtherSourceMatch(normalizedName);

            if (otherSourceMatch != Quality.Unknown)
            {
                result.Quality = otherSourceMatch;
            }

            return result;
        }

        private static Resolution ParseResolution(string name)
        {
            var match = ResolutionRegex.Match(name);

            if (!match.Success)
            {
                return Resolution.Unknown;
            }

            if (match.Groups["R480p"].Success)
            {
                return Resolution.R480p;
            }

            if (match.Groups["R576p"].Success)
            {
                return Resolution.R576p;
            }

            if (match.Groups["R720p"].Success)
            {
                return Resolution.R720p;
            }

            if (match.Groups["R1080p"].Success)
            {
                return Resolution.R1080p;
            }

            if (match.Groups["R2160p"].Success)
            {
                return Resolution.R2160p;
            }

            return Resolution.Unknown;
        }

        private static Quality OtherSourceMatch(string name)
        {
            var match = OtherSourceRegex.Match(name);

            if (!match.Success)
            {
                return Quality.Unknown;
            }

            if (match.Groups["sdtv"].Success)
            {
                return Quality.SDTV;
            }

            if (match.Groups["hdtv"].Success)
            {
                return Quality.HDTV720p;
            }

            return Quality.Unknown;
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

            var versionRegexResult = VersionRegex.Match(normalizedName);

            if (versionRegexResult.Success)
            {
                result.Revision.Version = Convert.ToInt32(versionRegexResult.Groups["version"].Value);
            }

            // TODO: re-enable this when we have a reliable way to determine real
            // TODO: Only treat it as a real if it comes AFTER the season/episode number
            var realRegexResult = RealRegex.Matches(name);

            if (realRegexResult.Count > 0)
            {
                result.Revision.Real = realRegexResult.Count;
            }

            return result;
        }
    }

    public enum Resolution
    {
        Unknown,
        R480p = 480,
        R576p = 576,
        R720p = 720,
        R1080p = 1080,
        R2160p = 2160
    }
}
