using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Test.Common;
using FluentAssertions;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshAlbumServiceFixture : CoreTest<RefreshAlbumService>
    {
        private Artist _artist;
        private List<Album> _albums;
        private List<AlbumRelease> _releases;
        private readonly string _fakeArtistForeignId = "xxx-xxx-xxx";
        private readonly List<ArtistMetadata> _fakeArtists = new List<ArtistMetadata> { new ArtistMetadata() };

        [SetUp]
        public void Setup()
        {

            var release = Builder<AlbumRelease>
                .CreateNew()
                .With(s => s.Media = new List<Medium> { new Medium { Number = 1 } })
                .With(s => s.ForeignReleaseId = "xxx-xxx-xxx-xxx")
                .With(s => s.Monitored = true)
                .With(s => s.TrackCount = 10)
                .Build();

            _releases = new List<AlbumRelease> { release };
            
            var album1 = Builder<Album>.CreateNew()
                .With(s => s.Id = 1234)
                .With(s => s.ForeignAlbumId = "1")
                .With(s => s.AlbumReleases = _releases)
                .Build();

            _albums = new List<Album>{ album1 };

            _artist = Builder<Artist>.CreateNew()
                .With(s => s.Albums = _albums)
                .Build();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);

            Mocker.GetMock<IReleaseService>()
                .Setup(s => s.GetReleasesForRefresh(album1.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(new List<AlbumRelease> { release });

            Mocker.GetMock<IArtistMetadataRepository>()
                .Setup(s => s.FindById(It.IsAny<List<string>>()))
                .Returns(new List<ArtistMetadata>());

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(It.IsAny<string>()))
                  .Callback(() => { throw new AlbumNotFoundException(album1.ForeignAlbumId); });

            Mocker.GetMock<ICheckIfAlbumShouldBeRefreshed>()
                .Setup(s => s.ShouldRefresh(It.IsAny<Album>()))
                .Returns(true);
        }

        private void GivenNewAlbumInfo(Album album)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(_albums.First().ForeignAlbumId))
                .Returns(new Tuple<string, Album, List<ArtistMetadata>>(_fakeArtistForeignId, album, _fakeArtists));
        }

        [Test]
        public void should_log_error_if_musicbrainz_id_not_found()
        {
            Subject.RefreshAlbumInfo(_albums, false, false);

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.IsAny<List<Album>>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed()
        {
            var newAlbumInfo = _albums.FirstOrDefault().JsonClone();
            newAlbumInfo.ForeignAlbumId = _albums.First().ForeignAlbumId + 1;
            newAlbumInfo.AlbumReleases = _releases;

            GivenNewAlbumInfo(newAlbumInfo);

            Subject.RefreshAlbumInfo(_albums, false, false);

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(s => s.First().ForeignAlbumId == newAlbumInfo.ForeignAlbumId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void two_equivalent_releases_should_be_equal()
        {
            var release = Builder<AlbumRelease>.CreateNew().Build();
            var release2 = Builder<AlbumRelease>.CreateNew().Build();

            ReferenceEquals(release, release2).Should().BeFalse();
            release.Equals(release2).Should().BeTrue();

            release.Label?.ToJson().Should().Be(release2.Label?.ToJson());
            release.Country?.ToJson().Should().Be(release2.Country?.ToJson());
            release.Media?.ToJson().Should().Be(release2.Media?.ToJson());
                                    
        }

        [Test]
        public void two_equivalent_tracks_should_be_equal()
        {
            var track = Builder<Track>.CreateNew().Build();
            var track2 = Builder<Track>.CreateNew().Build();

            ReferenceEquals(track, track2).Should().BeFalse();
            track.Equals(track2).Should().BeTrue();
        }

        [Test]
        public void two_equivalent_metadata_should_be_equal()
        {
            var meta = Builder<ArtistMetadata>.CreateNew().Build();
            var meta2 = Builder<ArtistMetadata>.CreateNew().Build();

            ReferenceEquals(meta, meta2).Should().BeFalse();
            meta.Equals(meta2).Should().BeTrue();
        }

        [Test]
        public void should_not_add_duplicate_releases()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew().Build();

            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumId = newAlbum.Id)
                .With(x => x.Monitored = true)
                .TheFirst(4)
                .With(x => x.ForeignReleaseId = "DuplicateId1")
                .TheLast(1)
                .With(x => x.ForeignReleaseId = "DuplicateId2")
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            albumCopy.AlbumReleases = releases;

            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(1)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "DuplicateId2")
                .With(x => x.Monitored = true)
                .Build() as List<AlbumRelease>;

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(x => x.GetAlbumInfo(It.IsAny<string>()))
                .Returns(Tuple.Create("dummy string", albumCopy, new List<ArtistMetadata>()));

            Subject.RefreshAlbumInfo(newAlbum, false);
            
            newAlbum.AlbumReleases.Value.Should().HaveCount(7);

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.DeleteMany(It.Is<List<AlbumRelease>>(l => l.Count == 0)), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.UpdateMany(It.Is<List<AlbumRelease>>(l => l.Count == 1 && l.Select(r => r.ForeignReleaseId).Distinct().Count() == 1)), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.InsertMany(It.Is<List<AlbumRelease>>(l => l.Count == 6 &&
                                                                    l.Select(r => r.ForeignReleaseId).Distinct().Count() == l.Count &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("DuplicateId2"))),
                        Times.Once());
        }
        
        [TestCase(true, true, 1)]
        [TestCase(true, false, 0)]
        [TestCase(false, true, 1)]
        [TestCase(false, false, 0)]
        public void should_only_leave_one_release_monitored(bool skyhookMonitored, bool existingMonitored, int expectedUpdates)
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew().Build();

            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumId = newAlbum.Id)
                .With(x => x.Monitored = skyhookMonitored)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "ExistingId2")
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            albumCopy.AlbumReleases = releases;

            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(2)
                .All()
                .With(x => x.Monitored = existingMonitored)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "ExistingId2")
                .Build() as List<AlbumRelease>;

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(x => x.GetAlbumInfo(It.IsAny<string>()))
                .Returns(Tuple.Create("dummy string", albumCopy, new List<ArtistMetadata>()));

            Subject.RefreshAlbumInfo(newAlbum, false);
            
            newAlbum.AlbumReleases.Value.Should().HaveCount(10);
            newAlbum.AlbumReleases.Value.Where(x => x.Monitored).Should().HaveCount(1);
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.DeleteMany(It.Is<List<AlbumRelease>>(l => l.Count == 0)), Times.Once());
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.UpdateMany(It.Is<List<AlbumRelease>>(l => l.Count == expectedUpdates && l.Select(r => r.ForeignReleaseId).Distinct().Count() == expectedUpdates)), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.InsertMany(It.Is<List<AlbumRelease>>(l => l.Count == 8 &&
                                                                    l.Select(r => r.ForeignReleaseId).Distinct().Count() == l.Count &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId1") &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId2"))),
                        Times.Once());
        }

        [Test]
        public void refreshing_album_should_not_change_monitored_release_if_monitored_release_not_deleted()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew().Build();

            // only ExistingId1 is monitored from dummy skyhook
            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumId = newAlbum.Id)
                .With(x => x.Monitored = false)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .With(x => x.Monitored = true)
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "ExistingId2")
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            albumCopy.AlbumReleases = releases;

            // ExistingId2 is monitored in DB
            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(2)
                .All()
                .With(x => x.Monitored = false)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "ExistingId2")
                .With(x => x.Monitored = true)
                .Build() as List<AlbumRelease>;

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(x => x.GetAlbumInfo(It.IsAny<string>()))
                .Returns(Tuple.Create("dummy string", albumCopy, new List<ArtistMetadata>()));

            Subject.RefreshAlbumInfo(newAlbum, false);

            newAlbum.AlbumReleases.Value.Should().HaveCount(10);
            newAlbum.AlbumReleases.Value.Where(x => x.Monitored).Should().HaveCount(1);
            newAlbum.AlbumReleases.Value.Single(x => x.Monitored).ForeignReleaseId.Should().Be("ExistingId2");
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.DeleteMany(It.Is<List<AlbumRelease>>(l => l.Count == 0)), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.UpdateMany(It.Is<List<AlbumRelease>>(l => l.Count == 0)), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.InsertMany(It.Is<List<AlbumRelease>>(l => l.Count == 8 &&
                                                                    l.Select(r => r.ForeignReleaseId).Distinct().Count() == l.Count &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId1") &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId2"))),
                        Times.Once());
        }
        
        [Test]
        public void refreshing_album_should_change_monitored_release_if_monitored_release_deleted()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew().Build();
            
            // Only existingId1 monitored in skyhook.  ExistingId2 is missing
            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumId = newAlbum.Id)
                .With(x => x.Monitored = false)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .With(x => x.Monitored = true)
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "NotExistingId2")
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            albumCopy.AlbumReleases = releases;

            // ExistingId2 is monitored but will be deleted
            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(2)
                .All()
                .With(x => x.Monitored = false)
                .TheFirst(1)
                .With(x => x.ForeignReleaseId = "ExistingId1")
                .TheNext(1)
                .With(x => x.ForeignReleaseId = "ExistingId2")
                .With(x => x.Monitored = true)
                .Build() as List<AlbumRelease>;

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(x => x.GetAlbumInfo(It.IsAny<string>()))
                .Returns(Tuple.Create("dummy string", albumCopy, new List<ArtistMetadata>()));

            Subject.RefreshAlbumInfo(newAlbum, false);

            newAlbum.AlbumReleases.Value.Should().HaveCount(10);
            newAlbum.AlbumReleases.Value.Where(x => x.Monitored).Should().HaveCount(1);
            newAlbum.AlbumReleases.Value.Single(x => x.Monitored).ForeignReleaseId.Should().NotBe("ExistingId2");

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.DeleteMany(It.Is<List<AlbumRelease>>(l => l.Single().ForeignReleaseId == "ExistingId2")), Times.Once());

            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.UpdateMany(It.Is<List<AlbumRelease>>(l => l.Count == 0)), Times.Once());
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.InsertMany(It.Is<List<AlbumRelease>>(l => l.Count == 9 &&
                                                                    l.Select(r => r.ForeignReleaseId).Distinct().Count() == l.Count &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId1") &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("ExistingId2"))),
                        Times.Once());
        }
    }
}
