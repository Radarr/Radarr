using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using Readarr.Http.REST;

namespace Readarr.Api.V1.BookFiles
{
    public class MediaInfoResource : RestResource
    {
        public decimal AudioChannels { get; set; }
        public string AudioBitRate { get; set; }
        public string AudioCodec { get; set; }
        public string AudioBits { get; set; }
        public string AudioSampleRate { get; set; }
    }

    public static class MediaInfoResourceMapper
    {
        public static MediaInfoResource ToResource(this MediaInfoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new MediaInfoResource
            {
                AudioChannels = MediaInfoFormatter.FormatAudioChannels(model),
                AudioCodec = MediaInfoFormatter.FormatAudioCodec(model),
                AudioBitRate = MediaInfoFormatter.FormatAudioBitrate(model),
                AudioBits = MediaInfoFormatter.FormatAudioBitsPerSample(model),
                AudioSampleRate = MediaInfoFormatter.FormatAudioSampleRate(model)
            };
        }
    }
}
