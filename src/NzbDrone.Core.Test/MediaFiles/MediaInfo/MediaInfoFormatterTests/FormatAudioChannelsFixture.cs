using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioChannelsFixture : TestBase
    {
        [Test]
        public void should_subtract_one_from_AudioChannels_as_total_channels_if_LFE_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 6,
                AudioChannelPositions = null,
                AudioChannelPositionsText = "Front: L C R, Side: L R, LFE"
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_use_AudioChannels_as_total_channels_if_LFE_not_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = "Front: L R"
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_return_0_if_schema_revision_is_less_than_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = null,
                SchemaRevision = 2
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(0);
        }

        [Test]
        public void should_use_AudioChannels_if_schema_revision_is_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "2/0/0",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions_including_decimal()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/0.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_handle_AudioChannelPositions_three_digits()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/0.2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_cleanup_extraneous_text_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "Object Based / 3/2/2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_skip_empty_groups_in_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = " / 2/0/0.0",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_first_series_of_numbers_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/2.1 / 3/2/2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_sum_first_series_of_numbers_from_AudioChannelPositions_with_three_digits()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/0.2.1 / 3/2/0.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_sum_dual_mono_representation_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "1+1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2.0m);
        }

        [Test]
        public void should_use_AudioChannelPositionText_when_AudioChannelChannelPosition_is_invalid()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 6,
                AudioChannelPositions = "15 objects",
                AudioChannelPositionsText = "15 objects / Front: L C R, Side: L R, LFE",
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_remove_atmos_objects_from_AudioChannelPostions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "15 objects / 3/2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }
    }
}
