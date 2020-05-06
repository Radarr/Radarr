using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AugmentQualityFixture : CoreTest<AggregateQuality>
    {
        private Mock<IAugmentQuality> _mediaInfoAugmenter;
        private Mock<IAugmentQuality> _fileExtensionAugmenter;
        private Mock<IAugmentQuality> _nameAugmenter;

        [SetUp]
        public void Setup()
        {
            _mediaInfoAugmenter = new Mock<IAugmentQuality>();
            _fileExtensionAugmenter = new Mock<IAugmentQuality>();
            _nameAugmenter = new Mock<IAugmentQuality>();

            _mediaInfoAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>()))
                               .Returns(AugmentQualityResult.ResolutionOnly((int)Resolution.R1080p, Confidence.MediaInfo));

            _fileExtensionAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>()))
                                   .Returns(new AugmentQualityResult(Source.TV, Confidence.Fallback, (int)Resolution.R720p, Confidence.Fallback, Modifier.NONE, Confidence.Fallback, new Revision()));

            _nameAugmenter.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>()))
                          .Returns(new AugmentQualityResult(Source.TV, Confidence.Default, (int)Resolution.R480p, Confidence.Default, Modifier.NONE, Confidence.Default, new Revision()));
        }

        private void GivenAugmenters(params Mock<IAugmentQuality>[] mocks)
        {
            Mocker.SetConstant<IEnumerable<IAugmentQuality>>(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_return_HDTV720_from_extension_when_other_augments_are_null()
        {
            var nullMock = new Mock<IAugmentQuality>();
            nullMock.Setup(s => s.AugmentQuality(It.IsAny<LocalMovie>()))
                    .Returns<LocalMovie>(l => null);

            GivenAugmenters(_fileExtensionAugmenter, nullMock);

            var result = Subject.Aggregate(new LocalMovie(), false);

            result.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_return_SDTV_when_HDTV720_came_from_extension()
        {
            GivenAugmenters(_fileExtensionAugmenter, _nameAugmenter);

            var result = Subject.Aggregate(new LocalMovie(), false);

            result.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.Quality.Quality.Should().Be(Quality.SDTV);
        }

        [Test]
        public void should_return_HDTV1080p_when_HDTV720_came_from_extension_and_mediainfo_indicates_1080()
        {
            GivenAugmenters(_fileExtensionAugmenter, _mediaInfoAugmenter);

            var result = Subject.Aggregate(new LocalMovie(), false);

            result.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.MediaInfo);
            result.Quality.Quality.Should().Be(Quality.HDTV1080p);
        }

        [Test]
        public void should_return_HDTV1080p_when_SDTV_came_from_name_and_mediainfo_indicates_1080()
        {
            GivenAugmenters(_nameAugmenter, _mediaInfoAugmenter);

            var result = Subject.Aggregate(new LocalMovie(), false);

            result.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.MediaInfo);
            result.Quality.Quality.Should().Be(Quality.HDTV1080p);
        }
    }
}
