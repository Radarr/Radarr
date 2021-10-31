using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FFMpegCore;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class MediaInfoModel : IEmbeddedDocument
    {
        private IMediaAnalysis _analysis;

        public string RawData { get; set; }
        public int SchemaRevision { get; set; }

        [JsonIgnore]
        public IMediaAnalysis Analysis
        {
            get
            {
                _analysis = FFProbe.Analyse(RawData);
                return _analysis;
            }
        }

        public string ContainerFormat { get; set; }
        public string VideoFormat { get; set; }

        public string VideoCodecID { get; set; }

        public string VideoProfile { get; set; }

        public int VideoBitrate { get; set; }

        public int VideoBitDepth { get; set; }

        public int VideoMultiViewCount { get; set; }

        public string VideoColourPrimaries { get; set; }

        public string VideoTransferCharacteristics { get; set; }
        public DoviConfigurationRecordSideData DoviConfigurationRecord { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public string AudioFormat { get; set; }

        public string AudioCodecID { get; set; }

        public string AudioProfile { get; set; }

        public int AudioBitrate { get; set; }

        public TimeSpan RunTime { get; set; }

        public int AudioStreamCount { get; set; }

        public int AudioChannels { get; set; }

        public string AudioChannelPositions { get; set; }

        public decimal VideoFps { get; set; }

        public List<string> AudioLanguages { get; set; }

        public List<string> Subtitles { get; set; }

        public string ScanType { get; set; }
    }
}
