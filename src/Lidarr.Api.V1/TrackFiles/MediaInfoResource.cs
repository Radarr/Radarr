using NzbDrone.Core.MediaFiles.MediaInfo;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.TrackFiles
{
    public class MediaInfoResource : RestResource
    {
        public decimal AudioChannels { get; set; }
        public string AudioBitRate { get; set; }
        public string AudioCodec { get; set; }
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
                       AudioChannels = MediaInfoFormatter.FormatAudioChannels(model),
                       AudioCodec = MediaInfoFormatter.FormatAudioCodec(model),
                       AudioBitRate = MediaInfoFormatter.FormatAudioBitrate(model)
                   };
        }
    }
}
