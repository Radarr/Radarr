using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.MediaInfo;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieFiles
{
    public class MediaInfoResource : RestResource
    {
        [Obsolete("Migrated to AudioStreams")]
        public long AudioBitrate { get; set; }
        [Obsolete("Migrated to AudioStreams")]
        public decimal AudioChannels { get; set; }
        [Obsolete("Migrated to AudioStreams")]
        public string AudioCodec { get; set; }
        [Obsolete("Migrated to AudioStreams")]
        public string AudioLanguages { get; set; }
        [Obsolete("Migrated to AudioStreams")]
        public int AudioStreamCount { get; set; }
        public int VideoBitDepth { get; set; }
        public long VideoBitrate { get; set; }
        public string VideoCodec { get; set; }
        public decimal VideoFps { get; set; }
        public string VideoDynamicRange { get; set; }
        public string VideoDynamicRangeType { get; set; }
        public string Resolution { get; set; }
        public string RunTime { get; set; }
        public string ScanType { get; set; }
        [Obsolete("Migrated to SubtitleStreams")]
        public string Subtitles { get; set; }
        public List<MediaInfoAudioStreamResource> AudioStreams { get; set; }
        public List<MediaInfoSubtitleStreamResource> SubtitleStreams { get; set; }
    }

    public static class MediaInfoResourceMapper
    {
        public static MediaInfoResource ToResource(this MediaInfoModel model, string sceneName)
        {
            if (model == null)
            {
                return null;
            }

            return new MediaInfoResource
            {
#pragma warning disable CS0618 // Type or member is obsolete
                AudioBitrate = model.PrimaryAudioStream?.Bitrate ?? 0,
                AudioChannels = MediaInfoFormatter.FormatAudioChannels(model.PrimaryAudioStream),
                AudioLanguages = model.AudioStreams?.Select(l => l.Language).ConcatToString("/"),
                AudioStreamCount = model.AudioStreams?.Count ?? 0,
                AudioCodec = MediaInfoFormatter.FormatAudioCodec(model.PrimaryAudioStream, sceneName),
#pragma warning restore CS0618 // Type or member is obsolete
                VideoBitDepth = model.VideoBitDepth,
                VideoBitrate = model.VideoBitrate,
                VideoCodec = MediaInfoFormatter.FormatVideoCodec(model, sceneName),
                VideoFps = Math.Round(model.VideoFps, 3),
                VideoDynamicRange = MediaInfoFormatter.FormatVideoDynamicRange(model),
                VideoDynamicRangeType = MediaInfoFormatter.FormatVideoDynamicRangeType(model),
                Resolution = $"{model.Width}x{model.Height}",
                RunTime = FormatRuntime(model.RunTime),
                ScanType = model.ScanType,
#pragma warning disable CS0618 // Type or member is obsolete
                Subtitles = model.SubtitleStreams?.Select(l => l.Language).ConcatToString("/"),
#pragma warning restore CS0618 // Type or member is obsolete
                AudioStreams = model.AudioStreams?.Select(stream => stream.ToResource(sceneName)).ToList(),
                SubtitleStreams = model.SubtitleStreams?.Select(stream => stream.ToResource()).ToList()
            };
        }

        private static string FormatRuntime(TimeSpan runTime)
        {
            var hours = (int)runTime.TotalHours;

            if (hours > 0)
            {
                return $"{hours}:{runTime.Minutes:00}:{runTime.Seconds:00}";
            }

            return $"{runTime.Minutes}:{runTime.Seconds:00}";
        }
    }
}
