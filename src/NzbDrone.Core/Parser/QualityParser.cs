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

        //private static readonly Regex SourceRegex = new Regex(@"\b(?:
        //                                                        (?<bluray>BluRay|Blu-Ray|HDDVD|BD)|
        //                                                        (?<webdl>WEB[-_. ]DL|WEBDL|WebRip|iTunesHD|WebHD|[. ]WEB[. ](?:[xh]26[45]|DD5[. ]1)|\d+0p[. ]WEB[. ])|
        //                                                        (?<hdtv>HDTV)|
        //                                                        (?<bdrip>BDRip)|
        //                                                        (?<brrip>BRRip)|
        //                                                        (?<dvd>DVD|DVDRip|NTSC|PAL|xvidvd)|
        //                                                        (?<dsr>WS[-_. ]DSR|DSR)|
        //                                                        (?<pdtv>PDTV)|
        //                                                        (?<sdtv>SDTV)|
        //                                                        (?<tvrip>TVRip)
        //                                                        )\b",
        //                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex SourceRegex = new Regex(@"\b(?:
                                                                (?<bluray>BluRay|Blu-Ray|HDDVD|BD)|
                                                                (?<webdl>WEB[-_. ]DL|HDRIP|WEBDL|WebRip|Web-Rip|iTunesHD|WebHD|[. ]WEB[. ](?:[xh]26[45]|DD5[. ]1)|\d+0p[. ]WEB[. ])|
                                                                (?<hdtv>HDTV)|
                                                                (?<bdrip>BDRip)|(?<brrip>BRRip)|
                                                                (?<dvdr>DVD-R|DVDR)|
                                                                (?<dvd>DVD|DVDRip|NTSC|PAL|xvidvd)|
                                                                (?<dsr>WS[-_. ]DSR|DSR)|
                                                                (?<regional>R[0-9]{1})|
                                                                (?<scr>SCR|SCREENER|DVDSCR|DVDSCREENER)|
                                                                (?<ts>TS|TELESYNC|HD-TS|HDTS|PDVD)|
                                                                (?<tc>TC|TELECINE|HD-TC|HDTC)|
                                                                (?<cam>CAMRIP|CAM|HDCAM|HD-CAM)|
                                                                (?<wp>WORKPRINT|WP)|
                                                                (?<pdtv>PDTV)|
                                                                (?<sdtv>SDTV)|
                                                                (?<tvrip>TVRip)
                                                                )\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex HardcodedSubsRegex = new Regex(@"\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC))\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex RemuxRegex = new Regex(@"\b(?<remux>Remux)\b",
                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ProperRegex = new Regex(@"\b(?<proper>proper|repack|rerip)\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new Regex(@"\dv(?<version>\d)\b|\[v(?<version>\d)\]",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RealRegex = new Regex(@"\b(?<real>REAL)\b",
                                                                RegexOptions.Compiled);

        private static readonly Regex ResolutionRegex = new Regex(@"\b(?:(?<R480p>480p|640x480|848x480)|(?<R576p>576p)|(?<R720p>720p|1280x720)|(?<R1080p>1080p|1920x1080)|(?<R2160p>2160p))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CodecRegex = new Regex(@"\b(?:(?<x264>x264)|(?<h264>h264)|(?<xvidhd>XvidHD)|(?<xvid>Xvid)|(?<divx>divx))\b",
                                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OtherSourceRegex = new Regex(@"(?<hdtv>HD[-_. ]TV)|(?<sdtv>SD[-_. ]TV)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AnimeBlurayRegex = new Regex(@"bd(?:720|1080)|(?<=[-_. (\[])bd(?=[-_. )\]])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HighDefPdtvRegex = new Regex(@"hr[-_. ]ws", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            if (RemuxRegex.IsMatch(normalizedName))
            {
                if (resolution == Resolution.R2160p)
                {
                    result.Quality = Quality.Remux2160p;
                    return result;
                }

                if (resolution == Resolution.R1080p)
                {
                    result.Quality = Quality.Remux1080p;
                    return result;
                }
            }

            if (sourceMatch != null && sourceMatch.Success)
            {
                if (sourceMatch.Groups["bluray"].Success)
                {
                    if (codecRegex.Groups["xvid"].Success || codecRegex.Groups["divx"].Success)
                    {
                        result.Quality = Quality.DVD;
                        return result;
                    }

                    if (resolution == Resolution.R2160p)
                    {
                        result.Quality = Quality.Bluray2160p;
                        return result;
                    }

                    if (resolution == Resolution.R1080p)
                    {
                        result.Quality = Quality.Bluray1080p;
                        return result;
                    }

                    if (resolution == Resolution.R576p)
                    {
                        result.Quality = Quality.Bluray576p;
                        return result;
                    }

                    if (resolution == Resolution.R480P)
                    {
                        result.Quality = Quality.DVD;
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
						case Resolution.R480P:
							result.Quality = Quality.Bluray480p;
							return result;
                        default:
                            result.Quality = Quality.DVD;
                            return result;
                    }
                }

                if (sourceMatch.Groups["wp"].Success)
                {
                    result.Quality = Quality.WORKPRINT;
                    return result;
                }

                if (sourceMatch.Groups["dvd"].Success)
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                if (sourceMatch.Groups["dvdr"].Success)
                {
                    result.Quality = Quality.DVDR;
                    return result;
                }

                if (sourceMatch.Groups["scr"].Success)
                {
                    result.Quality = Quality.DVDSCR;
                    return result;
                }

                if (sourceMatch.Groups["regional"].Success)
                {
                    result.Quality = Quality.REGIONAL;
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
                    return result;
                }

                if (sourceMatch.Groups["tc"].Success)
                {
                    result.Quality = Quality.TELECINE;
                    return result;
                }

                if (sourceMatch.Groups["pdtv"].Success ||
                    sourceMatch.Groups["sdtv"].Success ||
                    sourceMatch.Groups["dsr"].Success ||
                    sourceMatch.Groups["tvrip"].Success)
                {
                    if (HighDefPdtvRegex.IsMatch(normalizedName))
                    {
                        result.Quality = Quality.HDTV720p;
                        return result;
                    }

                    result.Quality = Quality.SDTV;
                    return result;
                }
            }

           


            //Anime Bluray matching
            if (AnimeBlurayRegex.Match(normalizedName).Success)
            {
                if (resolution == Resolution.R480P || resolution == Resolution.R576p || normalizedName.Contains("480p"))
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                if (resolution == Resolution.R1080p || normalizedName.Contains("1080p"))
                {
                    result.Quality = Quality.Bluray1080p;
                    return result;
                }

                result.Quality = Quality.Bluray720p;
                return result;
            }

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

            if (resolution == Resolution.R480P)
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

            var otherSourceMatch = OtherSourceMatch(normalizedName);

            if (otherSourceMatch != Quality.Unknown)
            {
                result.Quality = otherSourceMatch;
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

        private static Resolution ParseResolution(string name)
        {
            var match = ResolutionRegex.Match(name);

            if (!match.Success) return Resolution.Unknown;
            if (match.Groups["R480p"].Success) return Resolution.R480P;
            if (match.Groups["R576p"].Success) return Resolution.R576p;
            if (match.Groups["R720p"].Success) return Resolution.R720p;
            if (match.Groups["R1080p"].Success) return Resolution.R1080p;
            if (match.Groups["R2160p"].Success) return Resolution.R2160p;

            return Resolution.Unknown;
        }

        private static Quality OtherSourceMatch(string name)
        {
            var match = OtherSourceRegex.Match(name);

            if (!match.Success) return Quality.Unknown;
            if (match.Groups["sdtv"].Success) return Quality.SDTV;
            if (match.Groups["hdtv"].Success) return Quality.HDTV720p;

            return Quality.Unknown;
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

    public enum Resolution
    {
        R480P,
        R576p,
        R720p,
        R1080p,
        R2160p,
        Unknown
    }
}
