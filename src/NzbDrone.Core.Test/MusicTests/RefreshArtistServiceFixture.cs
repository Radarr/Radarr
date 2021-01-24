using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshAuthorServiceFixture : CoreTest<RefreshAuthorService>
    {
        private Author _author;
        private Book _book1;
        private Book _book2;
        private List<Book> _books;
        private List<Book> _remoteBooks;

        [SetUp]
        public void Setup()
        {
            _book1 = Builder<Book>.CreateNew()
                .With(s => s.ForeignBookId = "1")
                .Build();

            _book2 = Builder<Book>.CreateNew()
                .With(s => s.ForeignBookId = "2")
                .Build();

            _books = new List<Book> { _book1, _book2 };

            _remoteBooks = _books.JsonClone();
            _remoteBooks.ForEach(x => x.Id = 0);

            var metadata = Builder<AuthorMetadata>.CreateNew().Build();
            var series = Builder<Series>.CreateListOfSize(1).BuildList();
            var profile = Builder<MetadataProfile>.CreateNew().Build();

            _author = Builder<Author>.CreateNew()
                .With(a => a.Metadata = metadata)
                .With(a => a.Series = series)
                .With(a => a.MetadataProfile = profile)
                .Build();

            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                  .Setup(s => s.GetAuthors(new List<int> { _author.Id }))
                  .Returns(new List<Author> { _author });

            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .Setup(s => s.InsertMany(It.IsAny<List<Book>>()));

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.FilterBooks(It.IsAny<Author>(), It.IsAny<int>()))
                .Returns(_books);

            Mocker.GetMock<IProvideAuthorInfo>()
                .Setup(s => s.GetAuthorAndBooks(It.IsAny<string>(), It.IsAny<double>()))
                .Callback(() => { throw new AuthorNotFoundException(_author.ForeignAuthorId); });

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByAuthor(It.IsAny<int>()))
                .Returns(new List<BookFile>());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.GetByAuthor(It.IsAny<int>(), It.IsAny<HistoryEventType?>()))
                .Returns(new List<History.History>());

            Mocker.GetMock<IImportListExclusionService>()
                .Setup(x => x.FindByForeignId(It.IsAny<List<string>>()))
                .Returns(new List<ImportListExclusion>());

            Mocker.GetMock<IRootFolderService>()
                .Setup(x => x.All())
                .Returns(new List<RootFolder>());
        }

        private void GivenNewAuthorInfo(Author author)
        {
            Mocker.GetMock<IProvideAuthorInfo>()
                .Setup(s => s.GetAuthorAndBooks(_author.ForeignAuthorId, It.IsAny<double>()))
                .Returns(author);
        }

        private void GivenAuthorFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(x => x.GetFilesByAuthor(It.IsAny<int>()))
                  .Returns(Builder<BookFile>.CreateListOfSize(1).BuildList());
        }

        private void GivenBooksForRefresh(List<Book> books)
        {
            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .Setup(s => s.GetBooksForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(books);
        }

        private void AllowAuthorUpdate()
        {
            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .Setup(x => x.UpdateAuthor(It.IsAny<Author>()))
                .Returns((Author a) => a);
        }

        [Test]
        public void should_not_publish_author_updated_event_if_metadata_not_updated()
        {
            var newAuthorInfo = _author.JsonClone();
            newAuthorInfo.Metadata = _author.Metadata.Value.JsonClone();
            newAuthorInfo.Books = _remoteBooks;

            GivenNewAuthorInfo(newAuthorInfo);
            GivenBooksForRefresh(_books);
            AllowAuthorUpdate();

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            VerifyEventNotPublished<AuthorUpdatedEvent>();
            VerifyEventPublished<AuthorRefreshCompleteEvent>();
        }

        [Test]
        public void should_publish_author_updated_event_if_metadata_updated()
        {
            var newAuthorInfo = _author.JsonClone();
            newAuthorInfo.Metadata = _author.Metadata.Value.JsonClone();
            newAuthorInfo.Metadata.Value.Images = new List<MediaCover.MediaCover>
            {
                new MediaCover.MediaCover(MediaCover.MediaCoverTypes.Logo, "dummy")
            };
            newAuthorInfo.Books = _remoteBooks;

            GivenNewAuthorInfo(newAuthorInfo);
            GivenBooksForRefresh(new List<Book>());
            AllowAuthorUpdate();

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            VerifyEventPublished<AuthorUpdatedEvent>();
            VerifyEventPublished<AuthorRefreshCompleteEvent>();
        }

        [Test]
        public void should_log_error_and_delete_if_musicbrainz_id_not_found_and_author_has_no_files()
        {
            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.DeleteAuthor(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()));

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.IsAny<Author>()), Times.Never());

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.DeleteAuthor(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_log_error_but_not_delete_if_musicbrainz_id_not_found_and_author_has_files()
        {
            GivenAuthorFiles();
            GivenBooksForRefresh(new List<Book>());

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.IsAny<Author>()), Times.Never());

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.DeleteAuthor(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newAuthorInfo = _author.JsonClone();
            newAuthorInfo.Metadata = _author.Metadata.Value.JsonClone();
            newAuthorInfo.Books = _remoteBooks;
            newAuthorInfo.ForeignAuthorId = _author.ForeignAuthorId + 1;
            newAuthorInfo.Metadata.Value.Id = 100;

            GivenNewAuthorInfo(newAuthorInfo);

            var seq = new MockSequence();

            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .Setup(x => x.FindById(newAuthorInfo.ForeignAuthorId))
                .Returns(default(Author));

            // Make sure that the author is updated before we refresh the books
            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateAuthor(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetBooksForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Book>());

            // Update called twice for a move/merge
            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateAuthor(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.Is<Author>(s => s.AuthorMetadataId == 100 && s.ForeignAuthorId == newAuthorInfo.ForeignAuthorId)),
                        Times.Exactly(2));
        }

        [Test]
        public void should_merge_if_musicbrainz_id_changed_and_new_id_already_exists()
        {
            var existing = _author;

            var clash = _author.JsonClone();
            clash.Id = 100;
            clash.Metadata = existing.Metadata.Value.JsonClone();
            clash.Metadata.Value.Id = 101;
            clash.Metadata.Value.ForeignAuthorId = clash.Metadata.Value.ForeignAuthorId + 1;

            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .Setup(x => x.FindById(clash.Metadata.Value.ForeignAuthorId))
                .Returns(clash);

            var newAuthorInfo = clash.JsonClone();
            newAuthorInfo.Metadata = clash.Metadata.Value.JsonClone();
            newAuthorInfo.Books = _remoteBooks;

            GivenNewAuthorInfo(newAuthorInfo);

            var seq = new MockSequence();

            // Make sure that the author is updated before we refresh the books
            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetBooksByAuthor(existing.Id))
                .Returns(_books);

            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateMany(It.IsAny<List<Book>>()));

            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.DeleteAuthor(existing.Id, It.IsAny<bool>(), false));

            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateAuthor(It.Is<Author>(a => a.Id == clash.Id)))
                .Returns((Author a) => a);

            Mocker.GetMock<IBookService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetBooksForRefresh(clash.AuthorMetadataId, It.IsAny<IEnumerable<string>>()))
                .Returns(_books);

            // Update called twice for a move/merge
            Mocker.GetMock<IAuthorService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateAuthor(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Subject.Execute(new RefreshAuthorCommand(_author.Id));

            // the retained author gets updated
            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.Is<Author>(s => s.Id == clash.Id)), Times.Exactly(2));

            // the old one gets removed
            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.DeleteAuthor(existing.Id, false, false));

            Mocker.GetMock<IBookService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(x => x.Count == _books.Count)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
