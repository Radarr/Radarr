using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

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

            Subject.AugmentQuality(localMovie, null).Should().Be(null);
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

            Subject.AugmentQuality(localMovie, null).Should().Be(null);
        }

        [TestCase(4096, 1, Resolution.R2160p)] // True 4K
        [TestCase(4000, 1, Resolution.R2160p)]
        [TestCase(3840, 1, Resolution.R2160p)] // 4K UHD
        [TestCase(3200, 1, Resolution.R2160p)]
        [TestCase(2000, 1, Resolution.R1080p)]
        [TestCase(1920, 1, Resolution.R1080p)] // Full HD
        [TestCase(1440, 1080, Resolution.R1080p)] // 4:3 FullHD
        [TestCase(1800, 1, Resolution.R1080p)]
        [TestCase(1490, 1, Resolution.R720p)]
        [TestCase(1280, 1, Resolution.R720p)] // HD
        [TestCase(1200, 1, Resolution.R720p)]
        [TestCase(1024, 1, Resolution.R576p)]
        [TestCase(768, 576, Resolution.R576p)]
        [TestCase(800, 1, Resolution.R480p)]
        [TestCase(720, 1, Resolution.R480p)] // SDTV
        [TestCase(600, 1, Resolution.R480p)]
        [TestCase(100, 1, Resolution.R480p)]
        public void should_return_closest_resolution(int mediaInfoWidth, int mediaInfoHeight, Resolution expectedResolution)
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = mediaInfoWidth)
                                                   .With(m => m.Height = mediaInfoHeight)
                                                   .Build();

            var localMovie = Builder<LocalMovie>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localMovie, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be((int)expectedResolution);
            result.Source.Should().Be(QualitySource.UNKNOWN);
        }

        [Test]
        public void should_include_source_if_extracted_from_title()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.Width = 1920)
                .With(m => m.Height = 1080)
                .With(m => m.Title = "Movie.Title.2008.WEB.x264-Radarr")
                .Build();

            var localMovie = Builder<LocalMovie>.CreateNew()
                .With(l => l.MediaInfo = mediaInfo)
                .Build();

            var result = Subject.AugmentQuality(localMovie, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(1080);
            result.Source.Should().Be(QualitySource.WEBDL);
        }

        [Test]
        public void should_have_unknown_source_if_no_source_extracted_from_title()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.Width = 1920)
                .With(m => m.Height = 1080)
                .With(m => m.Title = "Movie.Title.2008.x264-Radarr")
                .Build();

            var localMovie = Builder<LocalMovie>.CreateNew()
                .With(l => l.MediaInfo = mediaInfo)
                .Build();

            var result = Subject.AugmentQuality(localMovie, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(1080);
            result.Source.Should().Be(QualitySource.UNKNOWN);
        }
    }
}
