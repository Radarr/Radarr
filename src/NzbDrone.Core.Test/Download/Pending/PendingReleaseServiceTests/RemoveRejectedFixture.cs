using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemoveRejectedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Artist _artist;
        private Album _album;
        private QualityProfile _profile;
        private ReleaseInfo _release;
        private ParsedAlbumInfo _parsedAlbumInfo;
        private RemoteAlbum _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .Build();

            _album = Builder<Album>.CreateNew()
                                       .Build();

            _profile = new QualityProfile
                       {
                           Name = "Test",
                           Cutoff = Quality.MP3_192.Id,
                           Items = new List<QualityProfileQualityItem>
                                   {
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_192 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_256 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 }
                                   },
                       };

            _artist.QualityProfile = new LazyLoaded<QualityProfile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedAlbumInfo = Builder<ParsedAlbumInfo>.CreateNew().Build();
            _parsedAlbumInfo.Quality = new QualityModel(Quality.MP3_192);

            _remoteAlbum = new RemoteAlbum();
            _remoteAlbum.Albums = new List<Album>{ _album };
            _remoteAlbum.Artist = _artist;
            _remoteAlbum.ParsedAlbumInfo = _parsedAlbumInfo;
            _remoteAlbum.Release = _release;

            _temporarilyRejected = new DownloadDecision(_remoteAlbum, new Rejection("Temp Rejected", RejectionType.Temporary));

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<PendingRelease>());

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(It.IsAny<int>()))
                  .Returns(_artist);

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtists(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Artist> { _artist });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetAlbums(It.IsAny<ParsedAlbumInfo>(), _artist, null))
                  .Returns(new List<Album> {_album});

            Mocker.GetMock<IPrioritizeDownloadDecision>()
                  .Setup(s => s.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns((List<DownloadDecision> d) => d);
        }

        private void GivenHeldRelease(string title, string indexer, DateTime publishDate)
        {
            var release = _release.JsonClone();
            release.Indexer = indexer;
            release.PublishDate = publishDate;


            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.ArtistId = _artist.Id)
                                                   .With(h => h.Title = title)
                                                   .With(h => h.Release = release)
                                                   .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(heldReleases);
        }

        [Test]
        public void should_remove_if_it_is_the_same_release_from_the_same_indexer()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate);

            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyDelete();
        }

        [Test]
        public void should_not_remove_if_title_is_different()
        {
            GivenHeldRelease(_release.Title + "-RP", _release.Indexer, _release.PublishDate);

            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyNoDelete();
        }

        [Test]
        public void should_not_remove_if_indexer_is_different()
        {
            GivenHeldRelease(_release.Title, "AnotherIndexer", _release.PublishDate);

            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyNoDelete();
        }

        [Test]
        public void should_not_remove_if_publish_date_is_different()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate.AddHours(1));

            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

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
