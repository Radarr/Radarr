using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromMediaInfoFixture : CoreTest<AugmentQualityFromMediaInfo>
    {
        [Test]
        public void should_return_null_if_media_info_is_null()
        {
            var localMovie = Builder<LocalMovie>.CreateNew()
                                                    .With(l => l.MediaInfo = null)
                                                    .Build();

            Subject.AugmentQuality(localMovie).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_media_info_width_is_zero()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 0)
                                                   .Build();

            var localMovie = Builder<LocalMovie>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            Subject.AugmentQuality(localMovie).Should().Be(null);
        }

        [TestCase(4096, Resolution.R2160P)] // True 4K
        [TestCase(4000, Resolution.R2160P)]
        [TestCase(3840, Resolution.R2160P)] // 4K UHD
        [TestCase(3200, Resolution.R2160P)]
        [TestCase(2000, Resolution.R1080P)]
        [TestCase(1920, Resolution.R1080P)] // Full HD
        [TestCase(1800, Resolution.R1080P)]
        [TestCase(1490, Resolution.R720P)]
        [TestCase(1280, Resolution.R720P)] // HD
        [TestCase(1200, Resolution.R720P)]
        [TestCase(800, Resolution.R480P)]
        [TestCase(720, Resolution.R480P)] // SDTV
        [TestCase(600, Resolution.R480P)]
        [TestCase(100, Resolution.R480P)]
        public void should_return_closest_resolution(int mediaInfoWidth, Resolution expectedResolution)
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = mediaInfoWidth)
                                                   .Build();

            var localMovie = Builder<LocalMovie>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localMovie);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(expectedResolution);
        }
    }
}
