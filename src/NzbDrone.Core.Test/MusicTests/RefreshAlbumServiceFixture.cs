using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshAlbumServiceFixture : CoreTest<RefreshBookService>
    {
        private Author _artist;
        private List<Book> _albums;

        [SetUp]
        public void Setup()
        {
            var album1 = Builder<Book>.CreateNew()
                .With(x => x.AuthorMetadata = Builder<AuthorMetadata>.CreateNew().Build())
                .With(s => s.Id = 1234)
                .With(s => s.ForeignBookId = "1")
                .Build();

            _albums = new List<Book> { album1 };

            _artist = Builder<Author>.CreateNew()
                .With(s => s.Books = _albums)
                .Build();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAuthor(_artist.Id))
                  .Returns(_artist);

            Mocker.GetMock<IAuthorMetadataService>()
                .Setup(s => s.UpsertMany(It.IsAny<List<AuthorMetadata>>()))
                .Returns(true);

            Mocker.GetMock<IProvideBookInfo>()
                .Setup(s => s.GetBookInfo(It.IsAny<string>()))
                  .Callback(() => { throw new BookNotFoundException(album1.ForeignBookId); });

            Mocker.GetMock<ICheckIfBookShouldBeRefreshed>()
                .Setup(s => s.ShouldRefresh(It.IsAny<Book>()))
                .Returns(true);

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByBook(It.IsAny<int>()))
                .Returns(new List<BookFile>());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.GetByBook(It.IsAny<int>(), It.IsAny<HistoryEventType?>()))
                .Returns(new List<History.History>());
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newAlbumInfo = _albums.First().JsonClone();
            newAlbumInfo.AuthorMetadata = _albums.First().AuthorMetadata.Value.JsonClone();
            newAlbumInfo.ForeignBookId = _albums.First().ForeignBookId + 1;

            Subject.RefreshBookInfo(_albums, new List<Book> { newAlbumInfo }, null, false, false, null);

            Mocker.GetMock<IBookService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(s => s.First().ForeignBookId == newAlbumInfo.ForeignBookId)));
        }

        [Test]
        public void should_merge_if_musicbrainz_id_changed_and_new_already_exists()
        {
            var existing = _albums.First();

            var clash = existing.JsonClone();
            clash.Id = 100;
            clash.AuthorMetadata = existing.AuthorMetadata.Value.JsonClone();
            clash.ForeignBookId += 1;

            Mocker.GetMock<IBookService>()
                .Setup(x => x.FindById(clash.ForeignBookId))
                .Returns(clash);

            var newAlbumInfo = existing.JsonClone();
            newAlbumInfo.AuthorMetadata = existing.AuthorMetadata.Value.JsonClone();
            newAlbumInfo.ForeignBookId = _albums.First().ForeignBookId + 1;

            Subject.RefreshBookInfo(_albums, new List<Book> { newAlbumInfo }, null, false, false, null);

            // check old album is deleted
            Mocker.GetMock<IBookService>()
                .Verify(v => v.DeleteMany(It.Is<List<Book>>(x => x.First().ForeignBookId == existing.ForeignBookId)));

            // check that clash gets updated
            Mocker.GetMock<IBookService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(s => s.First().ForeignBookId == newAlbumInfo.ForeignBookId)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
