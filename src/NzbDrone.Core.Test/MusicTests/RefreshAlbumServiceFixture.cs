using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshAlbumServiceFixture : CoreTest<RefreshAlbumService>
    {
        private readonly List<ArtistMetadata> _fakeArtists = new List<ArtistMetadata> { new ArtistMetadata() };
        private readonly string _fakeArtistForeignId = "xxx-xxx-xxx";
        private Artist _artist;
        private List<Album> _albums;
        private List<AlbumRelease> _releases;

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
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .With(s => s.Id = 1234)
                .With(s => s.ForeignAlbumId = "1")
                .With(s => s.AlbumReleases = _releases)
                .Build();

            _albums = new List<Album> { album1 };

            _artist = Builder<Artist>.CreateNew()
                .With(s => s.Albums = _albums)
                .Build();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);

            Mocker.GetMock<IReleaseService>()
                .Setup(s => s.GetReleasesForRefresh(album1.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(new List<AlbumRelease> { release });

            Mocker.GetMock<IArtistMetadataService>()
                .Setup(s => s.UpsertMany(It.IsAny<List<ArtistMetadata>>()))
                .Returns(true);

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(It.IsAny<string>()))
                  .Callback(() => { throw new AlbumNotFoundException(album1.ForeignAlbumId); });

            Mocker.GetMock<ICheckIfAlbumShouldBeRefreshed>()
                .Setup(s => s.ShouldRefresh(It.IsAny<Album>()))
                .Returns(true);

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByAlbum(It.IsAny<int>()))
                .Returns(new List<TrackFile>());

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByRelease(It.IsAny<int>()))
                .Returns(new List<TrackFile>());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.GetByAlbum(It.IsAny<int>(), It.IsAny<HistoryEventType?>()))
                .Returns(new List<History.History>());
        }

        private void GivenNewAlbumInfo(Album album)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(_albums.First().ForeignAlbumId))
                .Returns(new Tuple<string, Album, List<ArtistMetadata>>(_fakeArtistForeignId, album, _fakeArtists));
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newAlbumInfo = _albums.First().JsonClone();
            newAlbumInfo.ArtistMetadata = _albums.First().ArtistMetadata.Value.JsonClone();
            newAlbumInfo.ForeignAlbumId = _albums.First().ForeignAlbumId + 1;
            newAlbumInfo.AlbumReleases = _releases;

            GivenNewAlbumInfo(newAlbumInfo);

            Subject.RefreshAlbumInfo(_albums, null, false, false, null);

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(s => s.First().ForeignAlbumId == newAlbumInfo.ForeignAlbumId)));
        }

        [Test]
        public void should_merge_if_musicbrainz_id_changed_and_new_already_exists()
        {
            var existing = _albums.First();

            var clash = existing.JsonClone();
            clash.Id = 100;
            clash.ArtistMetadata = existing.ArtistMetadata.Value.JsonClone();
            clash.ForeignAlbumId = clash.ForeignAlbumId + 1;

            clash.AlbumReleases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All().With(x => x.AlbumId = clash.Id)
                .BuildList();

            Mocker.GetMock<IAlbumService>()
                .Setup(x => x.FindById(clash.ForeignAlbumId))
                .Returns(clash);

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesByAlbum(_albums.First().Id))
                .Returns(_releases);

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesByAlbum(clash.Id))
                .Returns(new List<AlbumRelease>());

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(_releases);

            var newAlbumInfo = existing.JsonClone();
            newAlbumInfo.ArtistMetadata = existing.ArtistMetadata.Value.JsonClone();
            newAlbumInfo.ForeignAlbumId = _albums.First().ForeignAlbumId + 1;
            newAlbumInfo.AlbumReleases = _releases;

            GivenNewAlbumInfo(newAlbumInfo);

            Subject.RefreshAlbumInfo(_albums, null, false, false, null);

            // check releases moved to clashing album
            Mocker.GetMock<IReleaseService>()
                .Verify(v => v.UpdateMany(It.Is<List<AlbumRelease>>(x => x.All(y => y.AlbumId == clash.Id) && x.Count == _releases.Count)));

            // check old album is deleted
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.DeleteMany(It.Is<List<Album>>(x => x.First().ForeignAlbumId == existing.ForeignAlbumId)));

            // check that clash gets updated
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(s => s.First().ForeignAlbumId == newAlbumInfo.ForeignAlbumId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remove_album_with_no_valid_releases()
        {
            var album = _albums.First();
            album.AlbumReleases = new List<AlbumRelease>();

            GivenNewAlbumInfo(album);

            Subject.RefreshAlbumInfo(album, null, false);

            Mocker.GetMock<IAlbumService>()
                .Verify(x => x.DeleteAlbum(album.Id, true, false),
                        Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_add_duplicate_releases()
        {
            var newAlbum = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

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

            Subject.RefreshAlbumInfo(newAlbum, null, false);

            Mocker.GetMock<IRefreshAlbumReleaseService>()
                .Verify(x => x.RefreshEntityInfo(It.Is<List<AlbumRelease>>(l => l.Count == 7 && l.Count(y => y.Monitored) == 1),
                                                 It.IsAny<List<AlbumRelease>>(),
                                                 It.IsAny<bool>(),
                                                 It.IsAny<bool>()));
        }

        [TestCase(true, true, 1)]
        [TestCase(true, false, 0)]
        [TestCase(false, true, 1)]
        [TestCase(false, false, 0)]
        public void should_only_leave_one_release_monitored(bool skyhookMonitored, bool existingMonitored, int expectedUpdates)
        {
            var newAlbum = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

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

            Subject.RefreshAlbumInfo(newAlbum, null, false);

            Mocker.GetMock<IRefreshAlbumReleaseService>()
                .Verify(x => x.RefreshEntityInfo(It.Is<List<AlbumRelease>>(l => l.Count == 10 && l.Count(y => y.Monitored) == 1),
                                                 It.IsAny<List<AlbumRelease>>(),
                                                 It.IsAny<bool>(),
                                                 It.IsAny<bool>()));
        }

        [Test]
        public void refreshing_album_should_not_change_monitored_release_if_monitored_release_not_deleted()
        {
            var newAlbum = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

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

            Subject.RefreshAlbumInfo(newAlbum, null, false);

            Mocker.GetMock<IRefreshAlbumReleaseService>()
                .Verify(x => x.RefreshEntityInfo(It.Is<List<AlbumRelease>>(
                                                     l => l.Count == 10 &&
                                                     l.Count(y => y.Monitored) == 1 &&
                                                     l.Single(y => y.Monitored).ForeignReleaseId == "ExistingId2"),
                                                 It.IsAny<List<AlbumRelease>>(),
                                                 It.IsAny<bool>(),
                                                 It.IsAny<bool>()));
        }

        [Test]
        public void refreshing_album_should_change_monitored_release_if_monitored_release_deleted()
        {
            var newAlbum = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

            // this is required because RefreshAlbumInfo will edit the album passed in
            var albumCopy = Builder<Album>.CreateNew()
                .With(x => x.ArtistMetadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();

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

            Subject.RefreshAlbumInfo(newAlbum, null, false);

            Mocker.GetMock<IRefreshAlbumReleaseService>()
                .Verify(x => x.RefreshEntityInfo(It.Is<List<AlbumRelease>>(
                                                     l => l.Count == 11 &&
                                                     l.Count(y => y.Monitored) == 1 &&
                                                     l.Single(y => y.Monitored).ForeignReleaseId != "ExistingId2"),
                                                 It.IsAny<List<AlbumRelease>>(),
                                                 It.IsAny<bool>(),
                                                 It.IsAny<bool>()));
        }
    }
}
