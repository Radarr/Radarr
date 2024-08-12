using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.MediaInfo;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieFiles
{
    public class MediaInfoResource : RestResource
    {
        public long AudioBitrate { get; set; }
        public decimal AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public string AudioLanguages { get; set; }
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
        public string Subtitles { get; set; }
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
                AudioBitrate = model.AudioBitrate,
                AudioChannels = MediaInfoFormatter.FormatAudioChannels(model),
                AudioLanguages = model.AudioLanguages.ConcatToString("/"),
                AudioStreamCount = model.AudioStreamCount,
                AudioCodec = MediaInfoFormatter.FormatAudioCodec(model, sceneName),
                VideoBitDepth = model.VideoBitDepth,
                VideoBitrate = model.VideoBitrate,
                VideoCodec = MediaInfoFormatter.FormatVideoCodec(model, sceneName),
                VideoFps = Math.Round(model.VideoFps, 3),
                VideoDynamicRange = MediaInfoFormatter.FormatVideoDynamicRange(model),
                VideoDynamicRangeType = MediaInfoFormatter.FormatVideoDynamicRangeType(model),
                Resolution = $"{model.Width}x{model.Height}",
                RunTime = FormatRuntime(model.RunTime),
                ScanType = model.ScanType,
                Subtitles = model.Subtitles.ConcatToString("/")
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
