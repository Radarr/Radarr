using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeFixture : TestBase
    {
        [TestCase(8, "", "", "")]
        [TestCase(8, "bt601 NTSC", "bt709", "")]
        [TestCase(10, "bt2020", "PQ", "HDR")]
        [TestCase(8, "bt2020", "PQ", "")]
        [TestCase(10, "bt601 NTSC", "PQ", "")]
        [TestCase(10, "bt2020", "bt709", "")]
        [TestCase(10, "bt2020", "HLG", "HDR")]
        [TestCase(10, "", "", "")]
        [TestCase(10, "bt2020", "PQ", "HDR")]
        public void should_format_video_dynamic_range(int bitDepth, string colourPrimaries, string transferCharacteristics, string expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoBitDepth = bitDepth,
                VideoColourPrimaries = colourPrimaries,
                VideoTransferCharacteristics = transferCharacteristics,
                SchemaRevision = 7
            };

            MediaInfoFormatter.FormatVideoDynamicRange(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }
    }
}
