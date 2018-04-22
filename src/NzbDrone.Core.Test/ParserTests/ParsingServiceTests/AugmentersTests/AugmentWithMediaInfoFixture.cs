using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithMediaInfoFixture : AugmentMovieInfoFixture<AugmentWithMediaInfo>
    {
        [TestCase(Resolution.R720P, Source.BLURAY, Resolution.R1080P)]
        [TestCase(Resolution.R1080P, Source.TV, Resolution.R720P)]
        public void should_correct_resolution(Resolution resolution, Source source, Resolution realResolution)
        {
            var quality = new QualityModel
            {
                Source = source,
                Resolution = resolution,
            };
            MovieInfo.Quality = quality;

            var realWidth = 480;
            switch (realResolution)
            {
                case Resolution.R720P:
                    realWidth = 1280;
                    break;
                case Resolution.R1080P:
                    realWidth = 1920;
                    break;
                case Resolution.R2160P:
                    realWidth = 2160;
                    break;

            }

            var mediaInfo = new MediaInfoModel
            {
                Width = realWidth
            };

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, mediaInfo);
            movieInfo.Quality.Resolution.ShouldBeEquivalentTo(realResolution);
            movieInfo.Quality.QualitySource.ShouldBeEquivalentTo(QualitySource.MediaInfo);
        }

        [TestCase(Resolution.R720P, Source.BLURAY, Resolution.R1080P, Modifier.BRDISK)]
        [TestCase(Resolution.R1080P, Source.BLURAY, Resolution.R720P, Modifier.REMUX)]
        [TestCase(Resolution.R480P, Source.BLURAY, Resolution.R720P)]
        [TestCase(Resolution.R720P, Source.DVD, Resolution.R480P)]
        public void should_not_correct_resolution(Resolution resolution, Source source, Resolution realResolution, Modifier modifier = Modifier.NONE)
        {
            var quality = new QualityModel
            {
                Source = source,
                Resolution = resolution,
                Modifier = modifier,
            };

            MovieInfo.Quality = quality;

            var realWidth = 480;
            switch (realResolution)
            {
                case Resolution.R720P:
                    realWidth = 1280;
                    break;
                case Resolution.R1080P:
                    realWidth = 1920;
                    break;
                case Resolution.R2160P:
                    realWidth = 2160;
                    break;

            }

            var mediaInfo = new MediaInfoModel
            {
                Width = realWidth
            };

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, mediaInfo);
            movieInfo.Quality.Resolution.ShouldBeEquivalentTo(resolution);
            movieInfo.Quality.QualitySource.ShouldBeEquivalentTo(QualitySource.Name);
        }
    }
}
