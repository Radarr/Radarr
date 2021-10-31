using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFMpegCore;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IVideoFileInfoReader
    {
        MediaInfoModel GetMediaInfo(string filename);
        TimeSpan? GetRunTime(string filename);
    }

    public class VideoFileInfoReader : IVideoFileInfoReader
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        private readonly List<FFProbePixelFormat> _pixelFormats;

        public const int MINIMUM_MEDIA_INFO_SCHEMA_REVISION = 8;
        public const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 8;

        public VideoFileInfoReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            // We bundle ffprobe for all platforms
            GlobalFFOptions.Configure(options => options.BinaryFolder = AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                _pixelFormats = FFProbe.GetPixelFormats();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get supported pixel formats from ffprobe");
                _pixelFormats = new List<FFProbePixelFormat>();
            }
        }

        public MediaInfoModel GetMediaInfo(string filename)
        {
            if (!_diskProvider.FileExists(filename))
            {
                throw new FileNotFoundException("Media file does not exist: " + filename);
            }

            // TODO: Cache media info by path, mtime and length so we don't need to read files multiple times
            try
            {
                _logger.Debug("Getting media info from {0}", filename);
                var ffprobeOutput = FFProbe.GetJson(filename, ffOptions: new FFOptions { ExtraArguments = "-probesize 50000000" });
                var analysis = FFProbe.AnalyseJson(ffprobeOutput);

                if (analysis.PrimaryAudioStream.ChannelLayout.IsNullOrWhiteSpace())
                {
                    ffprobeOutput = FFProbe.GetJson(filename, ffOptions: new FFOptions { ExtraArguments = "-probesize 150000000 -analyzeduration 150000000" });
                    analysis = FFProbe.AnalyseJson(ffprobeOutput);
                }

                var mediaInfoModel = new MediaInfoModel
                {
                    ContainerFormat = analysis.Format.FormatName,
                    VideoFormat = analysis.PrimaryVideoStream?.CodecName,
                    VideoCodecID = analysis.PrimaryVideoStream?.CodecTagString,
                    VideoProfile = analysis.PrimaryVideoStream?.Profile,
                    VideoBitrate = analysis.PrimaryVideoStream?.BitRate ?? 0,
                    VideoMultiViewCount = 1,
                    VideoBitDepth = GetPixelFormat(analysis.PrimaryVideoStream?.PixelFormat).Components.Min(x => x.BitDepth),
                    VideoColourPrimaries = analysis.PrimaryVideoStream?.ColorPrimaries,
                    VideoTransferCharacteristics = analysis.PrimaryVideoStream?.ColorTransfer,
                    DoviConfigurationRecord = analysis.PrimaryVideoStream?.SideDataList?.Find(x => x.GetType().Name == nameof(DoviConfigurationRecordSideData)) as DoviConfigurationRecordSideData,
                    Height = analysis.PrimaryVideoStream?.Height ?? 0,
                    Width = analysis.PrimaryVideoStream?.Width ?? 0,
                    AudioFormat = analysis.PrimaryAudioStream?.CodecName,
                    AudioCodecID = analysis.PrimaryAudioStream?.CodecTagString,
                    AudioProfile = analysis.PrimaryAudioStream?.Profile,
                    AudioBitrate = analysis.PrimaryAudioStream?.BitRate ?? 0,
                    RunTime = GetBestRuntime(analysis.PrimaryAudioStream?.Duration, analysis.PrimaryVideoStream.Duration, analysis.Format.Duration),
                    AudioStreamCount = analysis.AudioStreams.Count,
                    AudioChannels = analysis.PrimaryAudioStream?.Channels ?? 0,
                    AudioChannelPositions = analysis.PrimaryAudioStream?.ChannelLayout,
                    VideoFps = analysis.PrimaryVideoStream?.FrameRate ?? 0,
                    AudioLanguages = analysis.AudioStreams?.Select(x => x.Language)
                            .Where(l => l.IsNotNullOrWhiteSpace())
                            .ToList(),
                    Subtitles = analysis.SubtitleStreams?.Select(x => x.Language)
                            .Where(l => l.IsNotNullOrWhiteSpace())
                            .ToList(),
                    ScanType = "Progressive",
                    RawData = ffprobeOutput,
                    SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION
                };

                return mediaInfoModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to parse media info from file: {0}", filename);
            }

            return null;
        }

        public TimeSpan? GetRunTime(string filename)
        {
            var info = GetMediaInfo(filename);

            return info?.RunTime;
        }

        private static TimeSpan GetBestRuntime(TimeSpan? audio, TimeSpan? video, TimeSpan general)
        {
            if (!video.HasValue || video.Value.TotalMilliseconds == 0)
            {
                if (!audio.HasValue || audio.Value.TotalMilliseconds == 0)
                {
                    return general;
                }

                return audio.Value;
            }

            return video.Value;
        }

        private FFProbePixelFormat GetPixelFormat(string format)
        {
            return _pixelFormats.Find(x => x.Name == format);
        }
    }
}
