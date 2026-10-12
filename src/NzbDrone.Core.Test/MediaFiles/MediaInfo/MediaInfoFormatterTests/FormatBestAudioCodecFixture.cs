using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatBestAudioCodecFixture : TestBase
    {
        private static string sceneName = "My.Movie.2020-Group";

        [Test]
        public void should_pick_truehd_atmos_over_ac3()
        {
            var mediaInfo = new MediaInfoModel
            {
                BestAudioFormat = "truehd",
                BestAudioCodecID = "thd+",
                BestAudioProfile = string.Empty
            };

            MediaInfoFormatter.FormatAudioCodec(
                new MediaInfoModel
                {
                    AudioFormat = mediaInfo.BestAudioFormat,
                    AudioCodecID = mediaInfo.BestAudioCodecID,
                    AudioProfile = mediaInfo.BestAudioProfile
                },
                sceneName).Should().Be("TrueHD Atmos");
        }

        [Test]
        public void should_pick_dts_hd_ma_over_ac3()
        {
            var mediaInfo = new MediaInfoModel
            {
                BestAudioFormat = "dts",
                BestAudioCodecID = string.Empty,
                BestAudioProfile = "DTS-HD MA"
            };

            MediaInfoFormatter.FormatAudioCodec(
                new MediaInfoModel
                {
                    AudioFormat = mediaInfo.BestAudioFormat,
                    AudioCodecID = mediaInfo.BestAudioCodecID,
                    AudioProfile = mediaInfo.BestAudioProfile
                },
                sceneName).Should().Be("DTS-HD MA");
        }

        [Test]
        public void should_pick_dtsx_over_dts_hd_ma()
        {
            var mediaInfo = new MediaInfoModel
            {
                BestAudioFormat = "dts",
                BestAudioCodecID = string.Empty,
                BestAudioProfile = "DTS:X"
            };

            MediaInfoFormatter.FormatAudioCodec(
                new MediaInfoModel
                {
                    AudioFormat = mediaInfo.BestAudioFormat,
                    AudioCodecID = mediaInfo.BestAudioCodecID,
                    AudioProfile = mediaInfo.BestAudioProfile
                },
                sceneName).Should().Be("DTS-X");
        }

        [Test]
        public void should_return_single_stream_codec()
        {
            var mediaInfo = new MediaInfoModel
            {
                BestAudioFormat = "ac3",
                BestAudioCodecID = string.Empty,
                BestAudioProfile = string.Empty
            };

            MediaInfoFormatter.FormatAudioCodec(
                new MediaInfoModel
                {
                    AudioFormat = mediaInfo.BestAudioFormat,
                    AudioCodecID = mediaInfo.BestAudioCodecID,
                    AudioProfile = mediaInfo.BestAudioProfile
                },
                sceneName).Should().Be("AC3");
        }

        [Test]
        public void should_return_null_when_best_audio_format_is_null()
        {
            var mediaInfo = new MediaInfoModel
            {
                AudioFormat = null,
                AudioCodecID = null,
                AudioProfile = null
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfo, sceneName).Should().BeNull();
        }
    }
}
