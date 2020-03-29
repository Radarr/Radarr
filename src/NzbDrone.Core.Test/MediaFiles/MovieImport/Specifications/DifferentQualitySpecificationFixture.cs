using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class DifferentQualitySpecificationFixture : CoreTest<DifferentQualitySpecification>
    {
        private LocalMovie _localMovie;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            var qualityProfile = new Profile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p)
            };

            var fakeMovie = Builder<Movie>.CreateNew()
                                            .With(c => c.Profile = qualityProfile)
                                            .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Quality = new QualityModel(Quality.Bluray1080p))
                                                 .With(l => l.DownloadClientMovieInfo = new ParsedMovieInfo())
                                                 .With(l => l.Movie = fakeMovie)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        private void GivenGrabbedMovieHistory(QualityModel quality)
        {
            var history = Builder<MovieHistory>.CreateListOfSize(1)
                                                                 .TheFirst(1)
                                                                 .With(h => h.Quality = quality)
                                                                 .With(h => h.EventType = MovieHistoryEventType.Grabbed)
                                                                 .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_movie_history()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            _localMovie.Movie = Builder<Movie>.CreateNew()
                                              .With(e => e.MovieFileId = 0)
                                              .Build();

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_quality_matches()
        {
            GivenGrabbedMovieHistory(_localMovie.Quality);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_quality_does_not_match()
        {
            GivenGrabbedMovieHistory(new QualityModel(Quality.SDTV));

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
