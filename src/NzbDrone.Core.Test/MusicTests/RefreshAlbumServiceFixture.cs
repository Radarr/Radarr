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
        public void should_remove_items_from_list()
        {
            var releases = Builder<AlbumRelease>.CreateListOfSize(2).Build();
            var release = releases[0];
            releases.Remove(release);
            releases.Should().HaveCount(1);
        }
    }
}
