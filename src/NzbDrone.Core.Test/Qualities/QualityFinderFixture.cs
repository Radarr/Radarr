using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFinderFixture
    {
        [TestCase(Source.CAM, 480, Modifier.NONE)]
        [TestCase(Source.CAM, 1080, Modifier.NONE)]
        [TestCase(Source.CAM, 0, Modifier.NONE)]
        public void should_return_CAM(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.CAM);
        }

        [TestCase(Source.CAM, 1080, Modifier.SCREENER)]
        [TestCase(Source.CAM, 0, Modifier.SCREENER)]
        public void should_return_Unknown(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.Unknown);
        }

        [TestCase(Source.DVD, 480, Modifier.REMUX)]
        public void should_return_DVD_Remux(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.DVDR);
        }

        [TestCase(Source.DVD, 480, Modifier.NONE)]
        [TestCase(Source.DVD, 576, Modifier.NONE)]
        public void should_return_DVD(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.DVD);
        }

        [TestCase(Source.TV, 480, Modifier.NONE)]
        public void should_return_SDTV(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.SDTV);
        }

        [TestCase(Source.TV, 720, Modifier.NONE)]
        [TestCase(Source.UNKNOWN, 720, Modifier.NONE)]
        public void should_return_HDTV_720p(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV720p);
        }

        [TestCase(Source.TV, 1080, Modifier.NONE)]
        [TestCase(Source.UNKNOWN, 1080, Modifier.NONE)]
        public void should_return_HDTV_1080p(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV1080p);
        }

        [TestCase(Source.BLURAY, 720, Modifier.NONE)]
        public void should_return_Bluray720p(Source source, int resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.Bluray720p);
        }
    }
}
