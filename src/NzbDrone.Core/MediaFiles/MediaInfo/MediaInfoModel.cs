using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class MediaInfoModel : IEmbeddedDocument
    {
        public MediaInfoModel()
        {
            VideoStreams = new List<VideoInfoModel>();
            AudioStreams = new List<AudioInfoModel>();
        }

        public string ContainerFormat { get; set; }
        public TimeSpan RunTime { get; set; }
        public string Subtitles { get; set; }
        public int SchemaRevision { get; set; }
        public List<VideoInfoModel> VideoStreams { get; set; }
        public List<AudioInfoModel> AudioStreams { get; set; }
    }

    public class VideoInfoModel
    {
        public string VideoCodec { get; set; }
        public string VideoFormat { get; set; }
        public string VideoCodecID { get; set; }
        public string VideoProfile { get; set; }
        public string VideoCodecLibrary { get; set; }
        public int VideoBitrate { get; set; }
        public int VideoBitDepth { get; set; }
        public int VideoMultiViewCount { get; set; }
        public string VideoColourPrimaries { get; set; }
        public string VideoTransferCharacteristics { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal VideoFps { get; set; }
        public string ScanType { get; set; }
        public TimeSpan RunTime { get; set; }
    }

    public class AudioInfoModel
    {
        public string AudioFormat { get; set; }
        public string AudioCodecID { get; set; }
        public string AudioCodecLibrary { get; set; }
        public string AudioAdditionalFeatures { get; set; }
        public int AudioBitrate { get; set; }
        public int AudioChannels { get; set; }
        public string AudioChannelPositions { get; set; }
        public string AudioChannelPositionsText { get; set; }
        public string Language { get; set; }
        public string AudioProfile { get; set; }
        public TimeSpan RunTime { get; set; }
    }
}
