using System;
using NzbDrone.Core.MediaFiles.MediaInfo;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieFiles
{
    public class MediaInfoResource : RestResource
    {
        public string AudioAdditionalFeatures { get; set; }
        public int AudioBitrate { get; set; }
        public decimal AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public string AudioLanguages { get; set; }
        public int AudioStreamCount { get; set; }
        public int VideoBitDepth { get; set; }
        public int VideoBitrate { get; set; }
        public string VideoCodec { get; set; }
        public decimal VideoFps { get; set; }
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
                AudioAdditionalFeatures = model.AudioAdditionalFeatures,
                AudioBitrate = model.AudioBitrate,
                AudioChannels = MediaInfoFormatter.FormatAudioChannels(model),
                AudioLanguages = model.AudioLanguages,
                AudioStreamCount = model.AudioStreamCount,
                AudioCodec = MediaInfoFormatter.FormatAudioCodec(model, sceneName),
                VideoBitDepth = model.VideoBitDepth,
                VideoBitrate = model.VideoBitrate,
                VideoCodec = MediaInfoFormatter.FormatVideoCodec(model, sceneName),
                VideoFps = model.VideoFps,
                Resolution = $"{model.Width}x{model.Height}",
                RunTime = FormatRuntime(model.RunTime),
                ScanType = model.ScanType,
                Subtitles = model.Subtitles
            };
        }

        private static string FormatRuntime(TimeSpan runTime)
        {
            var formattedRuntime = "";

            if (runTime.Hours > 0)
            {
                formattedRuntime += $"{runTime.Hours}:";
            }

            formattedRuntime += $"{runTime.Minutes}:{runTime.Seconds}";

            return formattedRuntime;
        }
    }
}
