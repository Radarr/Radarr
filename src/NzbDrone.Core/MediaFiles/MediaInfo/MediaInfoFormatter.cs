using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static class MediaInfoFormatter
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MediaInfoFormatter));

        public static string FormatAudioBitrate(MediaInfoModel mediaInfo)
        {
            int audioBitrate = mediaInfo.AudioBitrate / 1000;

            return audioBitrate + " kbps";
        }

        public static decimal FormatAudioChannels(MediaInfoModel mediaInfo)
        {
            var audioChannelPositions = mediaInfo.AudioChannelPositions;
            var audioChannelPositionsText = mediaInfo.AudioChannelPositionsText;
            var audioChannels = mediaInfo.AudioChannels;

            if (audioChannelPositions.IsNullOrWhiteSpace())
            {
                if (audioChannelPositionsText.IsNullOrWhiteSpace())
                {
                    if (mediaInfo.SchemaRevision >= 3)
                    {
                        return audioChannels;
                    }

                    return 0;
                }

                return mediaInfo.AudioChannelPositionsText.ContainsIgnoreCase("LFE") ? audioChannels - 1 + 0.1m : audioChannels;
            }

            if (audioChannelPositions.Contains("+"))
            {
                return audioChannelPositions.Split('+')
                                            .Sum(s => decimal.Parse(s.Trim(), CultureInfo.InvariantCulture));
            }

            return audioChannelPositions.Replace("Object Based / ", "")
                                        .Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries)
                                        .First()
                                        .Split('/')
                                        .Sum(s => decimal.Parse(s, CultureInfo.InvariantCulture));
        }

        public static string FormatAudioCodec(MediaInfoModel mediaInfo)
        {
            if (mediaInfo.AudioCodecID == null)
            {
                return FormatAudioCodecLegacy(mediaInfo);
            }

            var audioFormat = mediaInfo.AudioFormat;
            var audioCodecID = mediaInfo.AudioCodecID ?? string.Empty;
            var audioProfile = mediaInfo.AudioProfile ?? string.Empty;
            var audioCodecLibrary = mediaInfo.AudioCodecLibrary ?? string.Empty;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (audioFormat.EqualsIgnoreCase("ALAC"))
            {
                return "ALAC";
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
                if (audioCodecID == "A_AAC/MPEG4/LC/SBR")
                {
                    return "HE-AAC";
                }

                return "AAC";
            }

            if (audioFormat.EqualsIgnoreCase("DTS"))
            {
                return "DTS";
            }

            if (audioFormat.EqualsIgnoreCase("FLAC"))
            {
                return "FLAC";
            }

            if (audioFormat.Trim().EqualsIgnoreCase("mp3"))
            {
                return "MP3";
            }

            if (audioFormat.EqualsIgnoreCase("MPEG Audio"))
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

            if (audioFormat.EqualsIgnoreCase("Opus"))
            {
                return "Opus";
            }

            if (audioFormat.EqualsIgnoreCase("PCM"))
            {
                return "PCM";
            }

            if (audioFormat.EqualsIgnoreCase("TrueHD"))
            {
                return "TrueHD";
            }

            if (audioFormat.EqualsIgnoreCase("Vorbis"))
            {
                return "Vorbis";
            }

            if (audioFormat == "WMA")
            {
                return "WMA";
            }

            Logger.Debug()
                  .Message("Unknown audio format: '{0}'.", string.Join(", ", audioFormat, audioCodecID, audioProfile, audioCodecLibrary))
                  .WriteSentryWarn("UnknownAudioFormat", mediaInfo.ContainerFormat, audioFormat, audioCodecID)
                  .Write();

            return audioFormat;
        }

        public static string FormatAudioCodecLegacy(MediaInfoModel mediaInfo)
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

            if (audioFormat.EqualsIgnoreCase("ALAC"))
            {
                return "ALAC";
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
    }
}
