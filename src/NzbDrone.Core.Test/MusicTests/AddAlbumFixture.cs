using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddAlbumFixture : CoreTest<AddAlbumService>
    {
        private Album _fakeAlbum;

        [SetUp]
        public void Setup()
        {
            _fakeAlbum = Builder<Album>
                .CreateNew()
                .Build();
        }

        private void GivenValidAlbum(string lidarrId)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                  .Setup(s => s.GetAlbumInfo(lidarrId, It.IsAny<string>()))
                  .Returns(new Tuple<Album, List<Track>>(_fakeAlbum, new List<Track>()));
        }

        [Test]
        public void should_be_able_to_add_an_album_without_passing_in_title()
        {
            var newAlbum = new Album
            {
                ForeignAlbumId = "ce09ea31-3d4a-4487-a797-e315175457a0"
            };

            GivenValidAlbum(newAlbum.ForeignAlbumId);

            var album = Subject.AddAlbum(newAlbum);

            album.Title.Should().Be(_fakeAlbum.Title);
        }

        [Test]
        public void should_throw_if_album_cannot_be_found()
        {
            var newAlbum = new Album
            {
                ForeignAlbumId = "ce09ea31-3d4a-4487-a797-e315175457a0"
            };

            Mocker.GetMock<IProvideAlbumInfo>()
                  .Setup(s => s.GetAlbumInfo(newAlbum.ForeignAlbumId, It.IsAny<string>()))
                  .Throws(new AlbumNotFoundException(newAlbum.ForeignAlbumId));

            Assert.Throws<ValidationException>(() => Subject.AddAlbum(newAlbum));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
