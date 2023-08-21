using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFinderFixture
    {
        [TestCase(QualitySource.CAM, 480, Modifier.NONE)]
        [TestCase(QualitySource.CAM, 1080, Modifier.NONE)]
        [TestCase(QualitySource.CAM, 0, Modifier.NONE)]
        public void should_return_CAM(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.CAM);
        }

        [TestCase(QualitySource.CAM, 1080, Modifier.SCREENER)]
        [TestCase(QualitySource.CAM, 0, Modifier.SCREENER)]
        public void should_return_Unknown(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.Unknown);
        }

        [TestCase(QualitySource.DVD, 480, Modifier.REMUX)]
        public void should_return_DVD_Remux(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.DVDR);
        }

        [TestCase(QualitySource.DVD, 480, Modifier.NONE)]
        [TestCase(QualitySource.DVD, 576, Modifier.NONE)]
        public void should_return_DVD(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.DVD);
        }

        [TestCase(QualitySource.TV, 480, Modifier.NONE)]
        public void should_return_SDTV(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.SDTV);
        }

        [TestCase(QualitySource.TV, 720, Modifier.NONE)]
        [TestCase(QualitySource.UNKNOWN, 720, Modifier.NONE)]
        public void should_return_HDTV_720p(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV720p);
        }

        [TestCase(QualitySource.TV, 1080, Modifier.NONE)]
        [TestCase(QualitySource.UNKNOWN, 1080, Modifier.NONE)]
        public void should_return_HDTV_1080p(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV1080p);
        }

        [TestCase(QualitySource.BLURAY, 720, Modifier.NONE)]
        public void should_return_Bluray720p(QualitySource source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.Bluray720p);
        }
    }
}
