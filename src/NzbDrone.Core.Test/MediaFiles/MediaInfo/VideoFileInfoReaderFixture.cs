using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    [DiskAccessTest]
    public class VideoFileInfoReaderFixture : CoreTest<VideoFileInfoReader>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.OpenReadStream(It.IsAny<string>()))
                  .Returns<string>(s => new FileStream(s, FileMode.Open, FileAccess.Read));
        }

        [Test]
        public void get_runtime()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            Subject.GetRunTime(path).Value.Seconds.Should().Be(10);
        }

        [Test]
        public void get_info()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var info = Subject.GetMediaInfo(path);

            var firstVideoStream = info.VideoStreams[0];
            var firstAudioStream = info.AudioStreams[0];

            firstVideoStream.VideoCodec.Should().BeNull();
            firstVideoStream.VideoFormat.Should().Be("AVC");
            firstVideoStream.VideoCodecID.Should().Be("avc1");
            firstVideoStream.VideoProfile.Should().Be("Baseline@L2.1");
            firstVideoStream.VideoCodecLibrary.Should().Be("");
            firstAudioStream.AudioFormat.Should().Be("AAC");
            firstAudioStream.AudioCodecID.Should().BeOneOf("40", "mp4a-40-2");
            firstAudioStream.AudioProfile.Should().BeOneOf("", "LC");
            firstAudioStream.AudioCodecLibrary.Should().Be("");
            firstAudioStream.AudioBitrate.Should().Be(128000);
            firstAudioStream.AudioChannels.Should().Be(2);
            firstAudioStream.Language.Should().Be("English");
            firstVideoStream.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            firstVideoStream.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().Be("");
            firstVideoStream.VideoBitrate.Should().Be(193329);
            firstVideoStream.VideoFps.Should().Be(24);
            firstVideoStream.Width.Should().Be(480);
            firstVideoStream.VideoColourPrimaries.Should().Be("BT.601 NTSC");
            firstVideoStream.VideoTransferCharacteristics.Should().Be("BT.709");
            firstAudioStream.AudioAdditionalFeatures.Should().BeOneOf("", "LC");
        }

        [Test]
        public void get_info_unicode()
        {
            var srcPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var tempPath = GetTempFilePath();
            Directory.CreateDirectory(tempPath);

            var path = Path.Combine(tempPath, "H264_Pok\u00E9mon.mkv");

            File.Copy(srcPath, path);

            var info = Subject.GetMediaInfo(path);

            var firstVideoStream = info.VideoStreams[0];
            var firstAudioStream = info.AudioStreams[0];

            firstVideoStream.VideoCodec.Should().BeNull();
            firstVideoStream.VideoFormat.Should().Be("AVC");
            firstVideoStream.VideoCodecID.Should().Be("avc1");
            firstVideoStream.VideoProfile.Should().Be("Baseline@L2.1");
            firstVideoStream.VideoCodecLibrary.Should().Be("");
            firstAudioStream.AudioFormat.Should().Be("AAC");
            firstAudioStream.AudioCodecID.Should().BeOneOf("40", "mp4a-40-2");
            firstAudioStream.AudioProfile.Should().BeOneOf("", "LC");
            firstAudioStream.AudioCodecLibrary.Should().Be("");
            firstAudioStream.AudioBitrate.Should().Be(128000);
            firstAudioStream.AudioChannels.Should().Be(2);
            firstAudioStream.Language.Should().Be("English");
            firstVideoStream.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            firstVideoStream.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().Be("");
            firstVideoStream.VideoBitrate.Should().Be(193329);
            firstVideoStream.VideoFps.Should().Be(24);
            firstVideoStream.Width.Should().Be(480);
            firstVideoStream.VideoColourPrimaries.Should().Be("BT.601 NTSC");
            firstVideoStream.VideoTransferCharacteristics.Should().Be("BT.709");
            firstAudioStream.AudioAdditionalFeatures.Should().BeOneOf("", "LC");
        }

        [Test]
        public void should_dispose_file_after_scanning_mediainfo()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var info = Subject.GetMediaInfo(path);

            var stream = new FileStream(path, FileMode.Open, FileAccess.Write);

            stream.Close();
        }
    }
}
