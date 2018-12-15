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

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshArtistServiceFixture : CoreTest<RefreshArtistService>
    {
        private Artist _artist;
        private Album _album1;
        private Album _album2;
        private List<Album> _albums;

        [SetUp]
        public void Setup()
        {
            _album1 = Builder<Album>.CreateNew()
                .With(s => s.ForeignAlbumId = "1")
                .Build();

            _album2 = Builder<Album>.CreateNew()
                .With(s => s.ForeignAlbumId = "2")
                .Build();

            _albums = new List<Album> {_album1, _album2};

            var metadata = Builder<ArtistMetadata>.CreateNew().Build();

            _artist = Builder<Artist>.CreateNew()
                .With(a => a.Metadata = metadata)
                .Build();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);

            Mocker.GetMock<IAlbumService>()
                .Setup(s => s.GetAlbumsByArtist(It.IsAny<int>()))
                .Returns(new List<Album>());
            
            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(It.IsAny<string>(), It.IsAny<int>()))
                  .Callback(() => { throw new ArtistNotFoundException(_artist.ForeignArtistId); });
        }

        private void GivenNewArtistInfo(Artist artist)
        {
            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(_artist.ForeignArtistId, _artist.MetadataProfileId))
                  .Returns(artist);
        }

        [Test]
        public void should_log_error_if_musicbrainz_id_not_found()
        {
            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Albums = _albums;
            newArtistInfo.ForeignArtistId = _artist.ForeignArtistId + 1;

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.ForeignArtistId == newArtistInfo.ForeignArtistId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        [Ignore("This test needs to be re-written as we no longer store albums in artist table or object")]
        public void should_not_throw_if_duplicate_album_is_in_existing_info()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Albums.Value.Add(Builder<Album>.CreateNew()
                                                  .With(s => s.ForeignAlbumId = "2")
                                                  .Build());

            _artist.Albums.Value.Add(Builder<Album>.CreateNew()
                                            .With(s => s.ForeignAlbumId = "2")
                                            .Build());

            _artist.Albums.Value.Add(Builder<Album>.CreateNew()
                                            .With(s => s.ForeignAlbumId = "2")
                                            .Build());

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Albums.Value.Count == 2)));
        }

        [Test]
        [Ignore("This test needs to be re-written as we no longer store albums in artist table or object")]
        public void should_filter_duplicate_albums()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Albums.Value.Add(Builder<Album>.CreateNew()
                                                  .With(s => s.ForeignAlbumId = "2")
                                                  .Build());

            newArtistInfo.Albums.Value.Add(Builder<Album>.CreateNew()
                                                  .With(s => s.ForeignAlbumId = "2")
                                                  .Build());

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Albums.Value.Count == 2)));

        }
    }
}
