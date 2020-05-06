using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddAlbumFixture : CoreTest<AddAlbumService>
    {
        private Author _fakeArtist;
        private Book _fakeAlbum;

        [SetUp]
        public void Setup()
        {
            _fakeAlbum = Builder<Book>
                .CreateNew()
                .Build();

            _fakeArtist = Builder<Author>
                .CreateNew()
                .With(s => s.Path = null)
                .With(s => s.Metadata = Builder<AuthorMetadata>.CreateNew().Build())
                .Build();
        }

        private void GivenValidAlbum(string readarrId)
        {
            Mocker.GetMock<IProvideBookInfo>()
                .Setup(s => s.GetBookInfo(readarrId))
                .Returns(Tuple.Create(_fakeArtist.Metadata.Value.ForeignAuthorId,
                                      _fakeAlbum,
                                      new List<AuthorMetadata> { _fakeArtist.Metadata.Value }));

            Mocker.GetMock<IAddArtistService>()
                .Setup(s => s.AddArtist(It.IsAny<Author>(), It.IsAny<bool>()))
                .Returns(_fakeArtist);
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Author>(), null))
                  .Returns<Author, NamingConfig>((c, n) => c.Name);
        }

        private Book AlbumToAdd(string bookId, string authorId)
        {
            return new Book
            {
                ForeignBookId = bookId,
                AuthorMetadata = new AuthorMetadata
                {
                    ForeignAuthorId = authorId
                }
            };
        }

        [Test]
        public void should_be_able_to_add_a_album_without_passing_in_name()
        {
            var newAlbum = AlbumToAdd("5537624c-3d2f-4f5c-8099-df916082c85c", "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");

            GivenValidAlbum(newAlbum.ForeignBookId);
            GivenValidPath();

            var album = Subject.AddAlbum(newAlbum);

            album.Title.Should().Be(_fakeAlbum.Title);
        }

        [Test]
        public void should_throw_if_album_cannot_be_found()
        {
            var newAlbum = AlbumToAdd("5537624c-3d2f-4f5c-8099-df916082c85c", "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");

            Mocker.GetMock<IProvideBookInfo>()
                  .Setup(s => s.GetBookInfo(newAlbum.ForeignBookId))
                  .Throws(new AlbumNotFoundException(newAlbum.ForeignBookId));

            Assert.Throws<ValidationException>(() => Subject.AddAlbum(newAlbum));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
