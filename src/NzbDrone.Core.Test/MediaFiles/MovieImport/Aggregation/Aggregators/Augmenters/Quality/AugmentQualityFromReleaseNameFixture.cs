using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromReleaseNameFixture : CoreTest<AugmentQualityFromReleaseName>
    {
        private LocalMovie _localMovie;
        private DownloadClientItem _downloadClientItem;
        private ParsedMovieInfo _hdtvParsedEpisodeInfo;
        private ParsedMovieInfo _webdlParsedEpisodeInfo;

        [SetUp]
        public void Setup()
        {
            _hdtvParsedEpisodeInfo = Builder<ParsedMovieInfo>.CreateNew()
                                                               .With(p => p.Quality =
                                                                   new QualityModel(Core.Qualities.Quality.HDTV720p))
                                                               .Build();

            _webdlParsedEpisodeInfo = Builder<ParsedMovieInfo>.CreateNew()
                                                                .With(p => p.Quality =
                                                                    new QualityModel(Core.Qualities.Quality.WEBDL720p))
                                                                .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        [Test]
        public void should_return_null_if_download_client_item_is_null()
        {
            Subject.AugmentQuality(_localMovie, null).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_no_grabbed_history()
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns((DownloadHistory)null);

            Subject.AugmentQuality(_localMovie, _downloadClientItem).Should().BeNull();
        }

        [TestCase("Series.Title.S01E01.1080p.WEB.x264", Source.WEBDL, Confidence.Tag, 1080, Confidence.Tag)]
        [TestCase("Series.Title.S01E01.WEB.x264", Source.WEBDL, Confidence.Tag, 480, Confidence.Fallback)]
        [TestCase("Series.Title.S01E01.720p.x264", Source.TV, Confidence.Fallback, 720, Confidence.Tag)]
        public void should_return_augmented_quality(string title, Source source, Confidence sourceConfidence, int resolution, Confidence resolutionConfidence)
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns(Builder<DownloadHistory>.CreateNew()
                                                   .With(h => h.SourceTitle = title)
                                                   .Build());

            var result = Subject.AugmentQuality(_localMovie, _downloadClientItem);

            result.Should().NotBe(null);
            result.Source.Should().Be(source);
            result.SourceConfidence.Should().Be(sourceConfidence);
            result.Resolution.Should().Be(resolution);
            result.ResolutionConfidence.Should().Be(resolutionConfidence);
        }
    }
}
