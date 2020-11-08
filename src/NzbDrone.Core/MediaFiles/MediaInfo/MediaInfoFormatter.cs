using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static class MediaInfoFormatter
    {
        private const string ValidHdrColourPrimaries = "BT.2020";
        private static readonly string[] ValidHdrTransferFunctions = { "PQ", "HLG" };

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MediaInfoFormatter));

        public static decimal FormatAudioChannels(MediaInfoModel mediaInfo)
        {
            var audioChannels = FormatAudioChannelsFromAudioChannelPositions(mediaInfo);

            if (audioChannels == null)
            {
                audioChannels = FormatAudioChannelsFromAudioChannelPositionsText(mediaInfo);
            }

            if (audioChannels == null)
            {
                audioChannels = FormatAudioChannelsFromAudioChannels(mediaInfo);
            }

            return audioChannels ?? 0;
        }

        public static string FormatAudioCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            if (mediaInfo.AudioCodecID == null)
            {
                return FormatAudioCodecLegacy(mediaInfo, sceneName);
            }

            var audioFormat = mediaInfo.AudioFormat.Trim().Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            var audioCodecID = mediaInfo.AudioCodecID ?? string.Empty;
            var audioProfile = mediaInfo.AudioProfile ?? string.Empty;
            var audioCodecLibrary = mediaInfo.AudioCodecLibrary ?? string.Empty;
            var splitAdditionalFeatures = (mediaInfo.AudioAdditionalFeatures ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (audioFormat.Empty())
            {
                return string.Empty;
            }

            if (audioFormat.ContainsIgnoreCase("Atmos"))
            {
                return "TrueHD Atmos";
            }

            if (audioFormat.ContainsIgnoreCase("MLP FBA"))
            {
                if (splitAdditionalFeatures.ContainsIgnoreCase("16-ch"))
                {
                    return "TrueHD Atmos";
                }

                return "TrueHD";
            }

            if (audioFormat.ContainsIgnoreCase("TrueHD"))
            {
                return "TrueHD";
            }

            if (audioFormat.ContainsIgnoreCase("FLAC"))
            {
                return "FLAC";
            }

            if (audioFormat.ContainsIgnoreCase("DTS"))
            {
                if (splitAdditionalFeatures.ContainsIgnoreCase("XLL"))
                {
                    if (splitAdditionalFeatures.ContainsIgnoreCase("X"))
                    {
                        return "DTS-X";
                    }

                    return "DTS-HD MA";
                }

                if (splitAdditionalFeatures.ContainsIgnoreCase("ES"))
                {
                    return "DTS-ES";
                }

                if (splitAdditionalFeatures.ContainsIgnoreCase("XBR"))
                {
                    return "DTS-HD HRA";
                }

                return "DTS";
            }

            if (audioFormat.ContainsIgnoreCase("E-AC-3"))
            {
                if (splitAdditionalFeatures.ContainsIgnoreCase("JOC"))
                {
                    return "EAC3 Atmos";
                }

                return "EAC3";
            }

            if (audioFormat.ContainsIgnoreCase("AC-3"))
            {
                return "AC3";
            }

            if (audioFormat.ContainsIgnoreCase("AAC"))
            {
                if (audioCodecID == "A_AAC/MPEG4/LC/SBR")
                {
                    return "HE-AAC";
                }

                return "AAC";
            }

            if (audioFormat.ContainsIgnoreCase("mp3"))
            {
                return "MP3";
            }

            if (audioFormat.ContainsIgnoreCase("MPEG Audio"))
            {
                if (mediaInfo.AudioCodecID == "55" || mediaInfo.AudioCodecID == "A_MPEG/L3" || mediaInfo.AudioProfile == "Layer 3")
                {
                    return "MP3";
                }

                if (mediaInfo.AudioCodecID == "A_MPEG/L2" || mediaInfo.AudioProfile == "Layer 2")
                {
                    return "MP2";
                }
            }

            if (audioFormat.ContainsIgnoreCase("Opus"))
            {
                return "Opus";
            }

            if (audioFormat.ContainsIgnoreCase("PCM"))
            {
                return "PCM";
            }

            if (audioFormat.ContainsIgnoreCase("ADPCM"))
            {
                return "PCM";
            }

            if (audioFormat.ContainsIgnoreCase("Vorbis"))
            {
                return "Vorbis";
            }

            if (audioFormat.ContainsIgnoreCase("WMA"))
            {
                return "WMA";
            }

            if (audioFormat.ContainsIgnoreCase("A_QUICKTIME"))
            {
                return "";
            }

            Logger.Debug()
                  .Message("Unknown audio format: '{0}' in '{1}'.", string.Join(", ", mediaInfo.AudioFormat, audioCodecID, audioProfile, audioCodecLibrary, mediaInfo.AudioAdditionalFeatures), sceneName)
                  .WriteSentryWarn("UnknownAudioFormat", mediaInfo.ContainerFormat, mediaInfo.AudioFormat, audioCodecID)
                  .Write();

            return mediaInfo.AudioFormat;
        }

        public static string FormatAudioCodecLegacy(MediaInfoModel mediaInfo, string sceneName)
        {
            var audioFormat = mediaInfo.AudioFormat;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                return audioFormat;
            }

            if (audioFormat.EqualsIgnoreCase("AC-3"))
            {
                return "AC3";
            }

            if (audioFormat.EqualsIgnoreCase("E-AC-3"))
            {
                return "EAC3";
            }

            if (audioFormat.EqualsIgnoreCase("AAC"))
            {
                return "AAC";
            }

            if (audioFormat.EqualsIgnoreCase("MPEG Audio") && mediaInfo.AudioProfile == "Layer 3")
            {
                return "MP3";
            }

            if (audioFormat.EqualsIgnoreCase("DTS"))
            {
                return "DTS";
            }

            if (audioFormat.EqualsIgnoreCase("TrueHD"))
            {
                return "TrueHD";
            }

            if (audioFormat.EqualsIgnoreCase("FLAC"))
            {
                return "FLAC";
            }

            if (audioFormat.EqualsIgnoreCase("Vorbis"))
            {
                return "Vorbis";
            }

            if (audioFormat.EqualsIgnoreCase("Opus"))
            {
                return "Opus";
            }

            return audioFormat;
        }

        public static string FormatVideoCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            if (mediaInfo.VideoFormat == null)
            {
                return FormatVideoCodecLegacy(mediaInfo, sceneName);
            }

            var videoFormat = mediaInfo.VideoFormat.Trim().Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            var videoCodecID = mediaInfo.VideoCodecID ?? string.Empty;
            var videoProfile = mediaInfo.VideoProfile ?? string.Empty;
            var videoCodecLibrary = mediaInfo.VideoCodecLibrary ?? string.Empty;

            var result = mediaInfo.VideoFormat.Trim();

            if (videoFormat.Empty())
            {
                return result;
            }

            if (videoFormat.ContainsIgnoreCase("x264"))
            {
                return "x264";
            }

            if (videoFormat.ContainsIgnoreCase("AVC") || videoFormat.ContainsIgnoreCase("V.MPEG4/ISO/AVC"))
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x264"))
                {
                    return "x264";
                }

                return GetSceneNameMatch(sceneName, "AVC", "x264", "h264");
            }

            if (videoFormat.ContainsIgnoreCase("HEVC") || videoFormat.ContainsIgnoreCase("V_MPEGH/ISO/HEVC"))
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x265"))
                {
                    return "x265";
                }

                return GetSceneNameMatch(sceneName, "HEVC", "x265", "h265");
            }

            if (videoFormat.ContainsIgnoreCase("MPEG Video"))
            {
                if (videoCodecID == "2" || videoCodecID == "V_MPEG2")
                {
                    return "MPEG2";
                }

                if (videoCodecID.IsNullOrWhiteSpace())
                {
                    return "MPEG";
                }
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-2 Video"))
            {
                return "MPEG2";
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-4 Visual"))
            {
                if (videoCodecID.ContainsIgnoreCase("XVID") ||
                    videoCodecLibrary.StartsWithIgnoreCase("XviD"))
                {
                    return "XviD";
                }

                if (videoCodecID.ContainsIgnoreCase("DIV3") ||
                    videoCodecID.ContainsIgnoreCase("DIVX") ||
                    videoCodecID.ContainsIgnoreCase("DX50") ||
                    videoCodecLibrary.StartsWithIgnoreCase("DivX"))
                {
                    return "DivX";
                }
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-4 Visual") || videoFormat.ContainsIgnoreCase("mp4v"))
            {
                result = GetSceneNameMatch(sceneName, "XviD", "DivX", "");
                if (result.IsNotNullOrWhiteSpace())
                {
                    return result;
                }

                if (videoCodecLibrary.Contains("Lavc"))
                {
                    return ""; // libavcodec mpeg-4
                }

                if (videoCodecLibrary.Contains("em4v"))
                {
                    return ""; // NeroDigital
                }

                if (videoCodecLibrary.Contains("Intel(R) IPP"))
                {
                    return ""; // Intel(R) IPP
                }

                if (videoCodecLibrary.Contains("ZJMedia") ||
                    videoCodecLibrary.Contains("DigiArty"))
                {
                    return ""; // Other
                }

                if (videoCodecLibrary == "")
                {
                    return ""; // Unknown mp4v
                }
            }

            if (videoFormat.ContainsIgnoreCase("VC-1"))
            {
                return "VC1";
            }

            if (videoFormat.ContainsIgnoreCase("AV1"))
            {
                return "AV1";
            }

            if (videoFormat.ContainsIgnoreCase("VP6") || videoFormat.ContainsIgnoreCase("VP7") ||
                videoFormat.ContainsIgnoreCase("VP8") || videoFormat.ContainsIgnoreCase("VP9"))
            {
                return videoFormat.First().ToUpperInvariant();
            }

            if (videoFormat.ContainsIgnoreCase("WMV1") || videoFormat.ContainsIgnoreCase("WMV2"))
            {
                return "WMV";
            }

            if (videoFormat.ContainsIgnoreCase("DivX") || videoFormat.ContainsIgnoreCase("div3"))
            {
                return "DivX";
            }

            if (videoFormat.ContainsIgnoreCase("XviD"))
            {
                return "XviD";
            }

            if (videoFormat.ContainsIgnoreCase("V_QUICKTIME") ||
                videoFormat.ContainsIgnoreCase("RealVideo 4"))
            {
                return "";
            }

            if (videoFormat.ContainsIgnoreCase("mp42") ||
                videoFormat.ContainsIgnoreCase("mp43"))
            {
                // MS old DivX competitor
                return "";
            }

            Logger.Debug()
                  .Message("Unknown video format: '{0}' in '{1}'.", string.Join(", ", mediaInfo.VideoFormat, videoCodecID, videoProfile, videoCodecLibrary), sceneName)
                  .WriteSentryWarn("UnknownVideoFormat", mediaInfo.ContainerFormat, mediaInfo.VideoFormat, videoCodecID)
                  .Write();

            return result;
        }

        public static string FormatVideoCodecLegacy(MediaInfoModel mediaInfo, string sceneName)
        {
            var videoCodec = mediaInfo.VideoCodec;

            if (videoCodec.IsNullOrWhiteSpace())
            {
                return videoCodec;
            }

            if (videoCodec == "AVC")
            {
                return GetSceneNameMatch(sceneName, "AVC", "h264", "x264");
            }

            if (videoCodec == "V_MPEGH/ISO/HEVC" || videoCodec == "HEVC")
            {
                return GetSceneNameMatch(sceneName, "HEVC", "h265", "x265");
            }

            if (videoCodec == "MPEG-2 Video")
            {
                return "MPEG2";
            }

            if (videoCodec == "MPEG-4 Visual")
            {
                return GetSceneNameMatch(sceneName, "DivX", "XviD");
            }

            if (videoCodec.StartsWithIgnoreCase("XviD"))
            {
                return "XviD";
            }

            if (videoCodec.StartsWithIgnoreCase("DivX"))
            {
                return "DivX";
            }

            if (videoCodec.EqualsIgnoreCase("VC-1"))
            {
                return "VC1";
            }

            return videoCodec;
        }

        private static decimal? FormatAudioChannelsFromAudioChannelPositions(MediaInfoModel mediaInfo)
        {
            var audioChannelPositions = mediaInfo.AudioChannelPositions;

            if (audioChannelPositions.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                if (audioChannelPositions.Contains("+"))
                {
                    return audioChannelPositions.Split('+')
                                                .Sum(s => decimal.Parse(s.Trim(), CultureInfo.InvariantCulture));
                }

                if (audioChannelPositions.Contains("/"))
                {
                    var channelStringList = Regex.Replace(audioChannelPositions,
                            @"^\d+\sobjects",
                            "",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase)
                        .Replace("Object Based / ", "")
                        .Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()
                        ?.Split('/');

                    var positions = default(decimal);

                    if (channelStringList == null)
                    {
                        return 0;
                    }

                    foreach (var channel in channelStringList)
                    {
                        var channelSplit = channel.Split(new string[] { "." }, StringSplitOptions.None);

                        if (channelSplit.Count() == 3)
                        {
                            positions += decimal.Parse(string.Format("{0}.{1}", channelSplit[1], channelSplit[2]), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            positions += decimal.Parse(channel, CultureInfo.InvariantCulture);
                        }
                    }

                    return positions;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn()
                      .Message("Unable to format audio channels using 'AudioChannelPositions', with a value of: '{0}' and '{1}'. Error {2}", audioChannelPositions, mediaInfo.AudioChannelPositionsText, ex.Message)
                      .WriteSentryWarn("UnknownAudioChannelFormat", audioChannelPositions, mediaInfo.AudioChannelPositionsText)
                      .Write();
            }

            return null;
        }

        private static decimal? FormatAudioChannelsFromAudioChannelPositionsText(MediaInfoModel mediaInfo)
        {
            var audioChannelPositionsText = mediaInfo.AudioChannelPositionsText;
            var audioChannels = mediaInfo.AudioChannels;

            if (audioChannelPositionsText.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                return audioChannelPositionsText.ContainsIgnoreCase("LFE") ? audioChannels - 1 + 0.1m : audioChannels;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Unable to format audio channels using 'AudioChannelPositionsText', with a value of: '{0}'", audioChannelPositionsText);
            }

            return null;
        }

        private static decimal? FormatAudioChannelsFromAudioChannels(MediaInfoModel mediaInfo)
        {
            var audioChannels = mediaInfo.AudioChannels;

            if (mediaInfo.SchemaRevision >= 3)
            {
                return audioChannels;
            }

            return null;
        }

        private static string GetSceneNameMatch(string sceneName, params string[] tokens)
        {
            sceneName = sceneName.IsNotNullOrWhiteSpace() ? Parser.Parser.RemoveFileExtension(sceneName) : string.Empty;

            foreach (var token in tokens)
            {
                if (sceneName.ContainsIgnoreCase(token))
                {
                    return token;
                }
            }

            // Last token is the default.
            return tokens.Last();
        }

        public static string FormatVideoDynamicRange(MediaInfoModel mediaInfo)
        {
            // assume SDR by default
            var videoDynamicRange = "";

            if (mediaInfo.VideoBitDepth >= 10 &&
                mediaInfo.VideoColourPrimaries.IsNotNullOrWhiteSpace() &&
                mediaInfo.VideoTransferCharacteristics.IsNotNullOrWhiteSpace())
            {
                if (mediaInfo.VideoColourPrimaries.EqualsIgnoreCase(ValidHdrColourPrimaries) &&
                    ValidHdrTransferFunctions.Any(mediaInfo.VideoTransferCharacteristics.Contains))
                {
                    videoDynamicRange = "HDR";
                }
            }

            return videoDynamicRange;
        }
    }
}
