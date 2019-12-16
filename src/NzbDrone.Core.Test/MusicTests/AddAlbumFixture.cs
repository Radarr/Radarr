using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddAlbumFixture : CoreTest<AddAlbumService>
    {
        private Artist _fakeArtist;
        private Album _fakeAlbum;

        [SetUp]
        public void Setup()
        {
            _fakeAlbum = Builder<Album>
                .CreateNew()
                .Build();

            _fakeArtist = Builder<Artist>
                .CreateNew()
                .With(s => s.Path = null)
                .With(s => s.Metadata = Builder<ArtistMetadata>.CreateNew().Build())
                .Build();
        }

        private void GivenValidAlbum(string lidarrId)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(lidarrId))
                .Returns(Tuple.Create(_fakeArtist.Metadata.Value.ForeignArtistId,
                                      _fakeAlbum,
                                      new List<ArtistMetadata> { _fakeArtist.Metadata.Value }));

            Mocker.GetMock<IAddArtistService>()
                .Setup(s => s.AddArtist(It.IsAny<Artist>(), It.IsAny<bool>()))
                .Returns(_fakeArtist);
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                  .Returns<Artist, NamingConfig>((c, n) => c.Name);
        }

        private Album AlbumToAdd(string albumId, string artistId)
        {
            return new Album
            {
                ForeignAlbumId = albumId,
                ArtistMetadata = new ArtistMetadata
                {
                    ForeignArtistId = artistId
                }
            };
        }

        [Test]
        public void should_be_able_to_add_a_album_without_passing_in_name()
        {
            var newAlbum = AlbumToAdd("5537624c-3d2f-4f5c-8099-df916082c85c", "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");

            GivenValidAlbum(newAlbum.ForeignAlbumId);
            GivenValidPath();

            var album = Subject.AddAlbum(newAlbum);

            album.Title.Should().Be(_fakeAlbum.Title);
        }

        [Test]
        public void should_throw_if_album_cannot_be_found()
        {
            var newAlbum = AlbumToAdd("5537624c-3d2f-4f5c-8099-df916082c85c", "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");

            Mocker.GetMock<IProvideAlbumInfo>()
                  .Setup(s => s.GetAlbumInfo(newAlbum.ForeignAlbumId))
                  .Throws(new AlbumNotFoundException(newAlbum.ForeignAlbumId));

            Assert.Throws<ValidationException>(() => Subject.AddAlbum(newAlbum));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
