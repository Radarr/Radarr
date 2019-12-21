using System;
using System.Globalization;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;

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

        public const int MINIMUM_MEDIA_INFO_SCHEMA_REVISION = 6;
        public const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 6;

        public VideoFileInfoReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public MediaInfoModel GetMediaInfo(string filename)
        {
            if (!_diskProvider.FileExists(filename))
            {
                throw new FileNotFoundException("Media file does not exist: " + filename);
            }

            MediaInfo mediaInfo = null;

            // TODO: Cache media info by path, mtime and length so we don't need to read files multiple times
            try
            {
                mediaInfo = new MediaInfo();
                _logger.Debug("Getting media info from {0}", filename);

                if (filename.ToLower().EndsWith(".ts"))
                {
                    mediaInfo.Option("ParseSpeed", "0.3");
                }
                else
                {
                    mediaInfo.Option("ParseSpeed", "0.0");
                }

                int open;

                using (var stream = _diskProvider.OpenReadStream(filename))
                {
                    open = mediaInfo.Open(stream);
                }

                if (open != 0)
                {
                    int audioRuntime;
                    int videoRuntime;
                    int generalRuntime;

                    //Runtime
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out videoRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "PlayTime"), out audioRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    if (audioRuntime == 0 && videoRuntime == 0 && generalRuntime == 0)
                    {
                        mediaInfo.Option("ParseSpeed", "1.0");

                        using (var stream = _diskProvider.OpenReadStream(filename))
                        {
                            open = mediaInfo.Open(stream);
                        }
                    }
                }

                if (open != 0)
                {
                    int generalRuntime;

                    int videoStreamCount = mediaInfo.Count_Get(StreamKind.Video);
                    int audioStreamCount = mediaInfo.Count_Get(StreamKind.Audio);

                    string subtitles = mediaInfo.Get(StreamKind.General, 0, "Text_Language_List");
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    var mediaInfoModel = new MediaInfoModel();

                    for (var videoIndex = 0; videoIndex < videoStreamCount; videoIndex++)
                    {
                        int width;
                        int height;
                        int videoBitRate;
                        int videoBitDepth;
                        decimal videoFrameRate;
                        int videoMultiViewCount;
                        int videoRuntime;

                        string scanType = mediaInfo.Get(StreamKind.Video, videoIndex, "ScanType");
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "Width"), out width);
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "Height"), out height);
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "BitRate"), out videoBitRate);

                        if (videoBitRate <= 0)
                        {
                            int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "BitRate_Nominal"), out videoBitRate);
                        }

                        decimal.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "FrameRate"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out videoFrameRate);
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "BitDepth"), out videoBitDepth);
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "MultiView_Count"), out videoMultiViewCount);

                        //Runtime
                        int.TryParse(mediaInfo.Get(StreamKind.Video, videoIndex, "PlayTime"), out videoRuntime);
                        string videoProfile = mediaInfo.Get(StreamKind.Video, videoIndex, "Format_Profile").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();

                        mediaInfoModel.VideoStreams.Add(new VideoInfoModel
                        {
                            VideoFormat = mediaInfo.Get(StreamKind.Video, videoIndex, "Format"),
                            VideoCodecID = mediaInfo.Get(StreamKind.Video, videoIndex, "CodecID"),
                            VideoProfile = videoProfile,
                            VideoCodecLibrary = mediaInfo.Get(StreamKind.Video, videoIndex, "Encoded_Library"),
                            VideoBitrate = videoBitRate,
                            VideoBitDepth = videoBitDepth,
                            VideoMultiViewCount = videoMultiViewCount,
                            VideoColourPrimaries = mediaInfo.Get(StreamKind.Video, videoIndex, "colour_primaries"),
                            VideoTransferCharacteristics = mediaInfo.Get(StreamKind.Video, videoIndex, "transfer_characteristics"),
                            Height = height,
                            Width = width,
                            VideoFps = videoFrameRate,
                            ScanType = scanType,
                            RunTime = TimeSpan.FromMilliseconds(videoRuntime),
                        });
                    }

                    for (var audioIndex = 0; audioIndex < audioStreamCount; audioIndex++)
                    {
                        int audioBitRate;
                        int audioRuntime;
                        int audioChannels;

                        int.TryParse(mediaInfo.Get(StreamKind.Audio, audioIndex, "PlayTime"), out audioRuntime);

                        string aBitRate = mediaInfo.Get(StreamKind.Audio, audioIndex, "BitRate").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();

                        int.TryParse(aBitRate, out audioBitRate);

                        string audioChannelsStr = mediaInfo.Get(StreamKind.Audio, audioIndex, "Channel(s)").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();

                        var audioChannelPositions = mediaInfo.Get(StreamKind.Audio, audioIndex, "ChannelPositions/String2");
                        var audioChannelPositionsText = mediaInfo.Get(StreamKind.Audio, audioIndex, "ChannelPositions");

                        string audioProfile = mediaInfo.Get(StreamKind.Audio, audioIndex, "Format_Profile").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();
                        int.TryParse(audioChannelsStr, out audioChannels);

                        mediaInfoModel.AudioStreams.Add(new AudioInfoModel
                        {
                            AudioFormat = mediaInfo.Get(StreamKind.Audio, audioIndex, "Format"),
                            AudioCodecID = mediaInfo.Get(StreamKind.Audio, audioIndex, "CodecID"),
                            AudioProfile = audioProfile,
                            AudioCodecLibrary = mediaInfo.Get(StreamKind.Audio, audioIndex, "Encoded_Library"),
                            AudioAdditionalFeatures = mediaInfo.Get(StreamKind.Audio, audioIndex, "Format_AdditionalFeatures"),
                            AudioBitrate = audioBitRate,
                            AudioChannels = audioChannels,
                            AudioChannelPositions = audioChannelPositions,
                            AudioChannelPositionsText = audioChannelPositionsText,
                            Language = mediaInfo.Get(StreamKind.Audio, audioIndex, "Language/String"),
                            RunTime = TimeSpan.FromMilliseconds(audioRuntime)
                        });
                    }

                    mediaInfoModel.ContainerFormat = mediaInfo.Get(StreamKind.General, 0, "Format");
                    mediaInfoModel.RunTime = TimeSpan.FromMilliseconds(generalRuntime);
                    mediaInfoModel.Subtitles = subtitles;
                    mediaInfoModel.SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION;

                    return mediaInfoModel;
                }
                else
                {
                    _logger.Warn("Unable to open media info from file: " + filename);
                }
            }
            catch (DllNotFoundException ex)
            {
                _logger.Error(ex, "mediainfo is required but was not found");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to parse media info from file: {0}", filename);
            }
            finally
            {
                mediaInfo?.Close();
            }

            return null;
        }

        public TimeSpan? GetRunTime(string filename)
        {
            var info = GetMediaInfo(filename);

            return info?.RunTime;
        }

        private TimeSpan GetBestRuntime(int audio, int video, int general)
        {
            if (video == 0)
            {
                if (audio == 0)
                {
                    return TimeSpan.FromMilliseconds(general);
                }

                return TimeSpan.FromMilliseconds(audio);
            }

            return TimeSpan.FromMilliseconds(video);
        }
    }
}
