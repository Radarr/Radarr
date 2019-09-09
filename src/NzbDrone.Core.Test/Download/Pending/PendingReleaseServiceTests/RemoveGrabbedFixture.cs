using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemoveGrabbedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Artist _artist;
        private Album _album;
        private QualityProfile _profile;
        private ReleaseInfo _release;
        private ParsedAlbumInfo _parsedAlbumInfo;
        private RemoteAlbum _remoteAlbum;
        private List<PendingRelease> _heldReleases;

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
                           Cutoff = Quality.MP3_256.Id,
                           Items = new List<QualityProfileQualityItem>
                                   {
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_256 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.FLAC }
                                   },
                       };

            _artist.QualityProfile = new LazyLoaded<QualityProfile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedAlbumInfo = Builder<ParsedAlbumInfo>.CreateNew().Build();
            _parsedAlbumInfo.Quality = new QualityModel(Quality.MP3_256);

            _remoteAlbum = new RemoteAlbum();
            _remoteAlbum.Albums = new List<Album>{ _album };
            _remoteAlbum.Artist = _artist;
            _remoteAlbum.ParsedAlbumInfo = _parsedAlbumInfo;
            _remoteAlbum.Release = _release;
            
            _temporarilyRejected = new DownloadDecision(_remoteAlbum, new Rejection("Temp Rejected", RejectionType.Temporary));

            _heldReleases = new List<PendingRelease>();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_heldReleases);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.AllByArtistId(It.IsAny<int>()))
                  .Returns<int>(i => _heldReleases.Where(v => v.ArtistId == i).ToList());

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

        private void GivenHeldRelease(QualityModel quality)
        {
            var parsedEpisodeInfo = _parsedAlbumInfo.JsonClone();
            parsedEpisodeInfo.Quality = quality;

            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.ArtistId = _artist.Id)
                                                   .With(h => h.Release = _release.JsonClone())
                                                   .With(h => h.ParsedAlbumInfo = parsedEpisodeInfo)
                                                   .Build();

            _heldReleases.AddRange(heldReleases);
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_same()
        {
            GivenHeldRelease(_parsedAlbumInfo.Quality);

            Subject.Handle(new AlbumGrabbedEvent(_remoteAlbum));

            VerifyDelete();
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_higher()
        {
            GivenHeldRelease(new QualityModel(Quality.MP3_192));

            Subject.Handle(new AlbumGrabbedEvent(_remoteAlbum));

            VerifyDelete();
        }

        [Test]
        public void should_not_delete_if_the_grabbed_quality_is_the_lower()
        {
            GivenHeldRelease(new QualityModel(Quality.FLAC));

            Subject.Handle(new AlbumGrabbedEvent(_remoteAlbum));

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
