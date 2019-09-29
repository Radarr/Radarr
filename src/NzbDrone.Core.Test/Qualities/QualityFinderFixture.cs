using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFinderFixture
    {
        [TestCase(Source.TV, 480, Modifier.NONE)]
        [TestCase(Source.UNKNOWN, 480, Modifier.NONE)]
        public void should_return_SDTV(Source source, Resolution resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.SDTV);
        }

        [TestCase(Source.TV, 720, Modifier.NONE)]
        [TestCase(Source.UNKNOWN, 720, Modifier.NONE)]
        public void should_return_HDTV_720p(Source source, Resolution resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV720p);
        }

        [TestCase(Source.TV, 1080, Modifier.NONE)]
        [TestCase(Source.UNKNOWN, 1080, Modifier.NONE)]
        public void should_return_HDTV_1080p(Source source, Resolution resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.HDTV1080p);
        }

        [TestCase(Source.BLURAY, 720, Modifier.NONE)]
        [TestCase(Source.DVD, 720, Modifier.NONE)]
        public void should_return_Bluray720p(Source source, Resolution resolution, Modifier modifier)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution, modifier).Should().Be(Quality.Bluray720p);
        }
    }
}