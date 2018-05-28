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
    public class RefreshAlbumServiceFixture : CoreTest<RefreshAlbumService>
    {
        private Artist _artist;
        private List<Album> _albums;

        [SetUp]
        public void Setup()
        {
            var album1 = Builder<Album>.CreateNew()
                .With(s => s.ForeignAlbumId = "1")
                .Build();

            _albums = new List<Album>{album1};

            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Albums = new List<Album>
                                                            {
                                                                album1
                                                            })
                                     .Build();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);
            
            Mocker.GetMock<IProvideAlbumInfo>()
                  .Setup(s => s.GetAlbumInfo(It.IsAny<string>(), It.IsAny<string>()))
                  .Callback(() => { throw new AlbumNotFoundException(album1.ForeignAlbumId); });

            Mocker.GetMock<ICheckIfAlbumShouldBeRefreshed>()
                .Setup(s => s.ShouldRefresh(It.IsAny<Album>()))
                .Returns(true);
        }

        private void GivenNewAlbumInfo(Album album)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                  .Setup(s => s.GetAlbumInfo(_albums.First().ForeignAlbumId, It.IsAny<string>()))
                  .Returns(new Tuple<Album, List<Track>>(album, new List<Track>()));
        }

        [Test]
        public void should_log_error_if_musicbrainz_id_not_found()
        {
            Subject.RefreshAlbumInfo(_albums, false);

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.IsAny<List<Album>>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed()
        {
            var newAlbumInfo = _albums.FirstOrDefault().JsonClone();
            newAlbumInfo.ForeignAlbumId = _albums.First().ForeignAlbumId + 1;

            GivenNewAlbumInfo(newAlbumInfo);

            Subject.RefreshAlbumInfo(_albums, false);

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(s => s.First().ForeignAlbumId == newAlbumInfo.ForeignAlbumId)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
