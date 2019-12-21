using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.MediaFiles.MediaInfo;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieFiles
{
    public class MediaInfoResource : RestResource
    {
        public string RunTime { get; set; }
        public string Subtitles { get; set; }
        public List<AudioInfoModel> AudioStreams { get; set; }
        public List<VideoInfoModel> VideoStreams { get; set; }
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
                AudioStreams = model.AudioStreams,
                VideoStreams = model.VideoStreams,
                RunTime = FormatRuntime(model.RunTime),
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
