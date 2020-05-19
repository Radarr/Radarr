using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemoveGrabbedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Author _artist;
        private Book _album;
        private QualityProfile _profile;
        private ReleaseInfo _release;
        private ParsedBookInfo _parsedBookInfo;
        private RemoteBook _remoteBook;
        private List<PendingRelease> _heldReleases;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Author>.CreateNew()
                                     .Build();

            _album = Builder<Book>.CreateNew()
                                       .Build();

            _profile = new QualityProfile
            {
                Name = "Test",
                Cutoff = Quality.MP3_320.Id,
                Items = new List<QualityProfileQualityItem>
                                   {
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.FLAC }
                                   },
            };

            _artist.QualityProfile = new LazyLoaded<QualityProfile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedBookInfo = Builder<ParsedBookInfo>.CreateNew().Build();
            _parsedBookInfo.Quality = new QualityModel(Quality.MP3_320);

            _remoteBook = new RemoteBook();
            _remoteBook.Books = new List<Book> { _album };
            _remoteBook.Author = _artist;
            _remoteBook.ParsedBookInfo = _parsedBookInfo;
            _remoteBook.Release = _release;

            _temporarilyRejected = new DownloadDecision(_remoteBook, new Rejection("Temp Rejected", RejectionType.Temporary));

            _heldReleases = new List<PendingRelease>();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_heldReleases);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.AllByAuthorId(It.IsAny<int>()))
                  .Returns<int>(i => _heldReleases.Where(v => v.AuthorId == i).ToList());

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAuthor(It.IsAny<int>()))
                  .Returns(_artist);

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAuthors(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Author> { _artist });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetAlbums(It.IsAny<ParsedBookInfo>(), _artist, null))
                  .Returns(new List<Book> { _album });

            Mocker.GetMock<IPrioritizeDownloadDecision>()
                  .Setup(s => s.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns((List<DownloadDecision> d) => d);
        }

        private void GivenHeldRelease(QualityModel quality)
        {
            var parsedEpisodeInfo = _parsedBookInfo.JsonClone();
            parsedEpisodeInfo.Quality = quality;

            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.AuthorId = _artist.Id)
                                                   .With(h => h.Release = _release.JsonClone())
                                                   .With(h => h.ParsedBookInfo = parsedEpisodeInfo)
                                                   .Build();

            _heldReleases.AddRange(heldReleases);
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_same()
        {
            GivenHeldRelease(_parsedBookInfo.Quality);

            Subject.Handle(new BookGrabbedEvent(_remoteBook));

            VerifyDelete();
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_higher()
        {
            GivenHeldRelease(new QualityModel(Quality.MP3_320));

            Subject.Handle(new BookGrabbedEvent(_remoteBook));

            VerifyDelete();
        }

        [Test]
        public void should_not_delete_if_the_grabbed_quality_is_the_lower()
        {
            GivenHeldRelease(new QualityModel(Quality.FLAC));

            Subject.Handle(new BookGrabbedEvent(_remoteBook));

            VerifyNoDelete();
        }

        private void VerifyDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Once());
        }

        private void VerifyNoDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Never());
        }
    }
}
