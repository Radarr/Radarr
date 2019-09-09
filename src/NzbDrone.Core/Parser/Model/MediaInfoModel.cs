using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Parser.Model
{
    public class MediaInfoModel : IEmbeddedDocument
    {
        public string AudioFormat { get; set; }
        public int AudioBitrate { get; set; }
        public int AudioChannels { get; set; }
        public int AudioBits { get; set; }
        public int AudioSampleRate { get; set; }
    }
}
