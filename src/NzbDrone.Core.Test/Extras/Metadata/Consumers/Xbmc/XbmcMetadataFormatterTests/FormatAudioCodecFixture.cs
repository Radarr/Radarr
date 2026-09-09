using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata.Consumers.Xbmc;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Xbmc.XbmcMetadataFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        [TestCase(new[] { "dts", "", "DTS-HD MA" }, "dtshd_ma")]
        [TestCase(new[] { "dts", "", "DTS:X" }, "dtshd_ma_x")]
        [TestCase(new[] { "dts", "", "DTS" }, "dts")]
        [TestCase(new[] { "truehd", "thd+", "" }, "truehd_atmos")]
        [TestCase(new[] { "truehd", "", "" }, "truehd")]
        [TestCase(new[] { "eac3", "ec+3", "" }, "eac3_ddp_atmos")]
        [TestCase(new[] { "eac3", "", "" }, "eac3")]
        [TestCase(new[] { "aac", "", "" }, "aac")]
        [TestCase(new[] { "aac", "", "HE-AAC" }, "he_aac")]
        public void should_format_audio_format(string[] audioFormat, string expectedFormat)
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = audioFormat[0],
                AudioCodecID = audioFormat[1],
                AudioProfile = audioFormat[2]
            };

            XbmcMetadataFormatter.FormatAudioCodec(mediaInfoModel).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_audio_format_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "Other Audio Format",
            };

            XbmcMetadataFormatter.FormatAudioCodec(mediaInfoModel).Should().Be(mediaInfoModel.AudioFormat);
        }

        [Test]
        public void should_return_empty_if_audio_stream_is_null()
        {
            XbmcMetadataFormatter.FormatAudioCodec(null).Should().Be(string.Empty);
        }
    }
}
