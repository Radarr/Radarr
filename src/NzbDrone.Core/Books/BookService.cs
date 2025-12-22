using System;
using System.Collections.Generic;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface IBookService : IBaseMediaService<Book>
    {
        Book GetBook(int bookId);
        List<Book> GetBooks(IEnumerable<int> bookIds);
        Book AddBook(Book newBook);
        List<Book> AddBooks(List<Book> newBooks);
        Book FindByIsbn(string isbn);
        Book FindByIsbn13(string isbn13);
        Book FindByAsin(string asin);
        Book FindByForeignId(string foreignBookId);
        Book FindByPath(string path);
        List<Book> FindByAuthorId(int authorId);
        List<Book> FindBySeriesId(int seriesId);
        Dictionary<int, string> AllBookPaths();
        List<Book> GetBooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        void DeleteBook(int bookId, bool deleteFiles);
        void DeleteBooks(List<int> bookIds, bool deleteFiles);
        List<Book> GetAllBooks();
        Dictionary<int, List<int>> AllBookTags();
        Book UpdateBook(Book book);
        List<Book> UpdateBooks(List<Book> books);
        bool BookPathExists(string folder);
    }

    public class BookService : BaseMediaService<Book>, IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IEventAggregator _eventAggregator;

        public BookService(IBookRepository bookRepository, IEventAggregator eventAggregator)
        {
            _bookRepository = bookRepository;
            _eventAggregator = eventAggregator;
        }

        protected override IBasicRepository<Book> Repository => _bookRepository;
        protected override IEventAggregator EventAggregator => _eventAggregator;

        public Book GetBook(int bookId) => Get(bookId);
        public List<Book> GetBooks(IEnumerable<int> bookIds) => Get(bookIds);
        public Book AddBook(Book newBook) => Add(newBook);
        public List<Book> AddBooks(List<Book> newBooks) => AddMany(newBooks);
        public void DeleteBook(int bookId, bool deleteFiles) => Delete(bookId, deleteFiles);
        public void DeleteBooks(List<int> bookIds, bool deleteFiles) => DeleteMany(bookIds, deleteFiles);
        public List<Book> GetAllBooks() => GetAll();
        public Book UpdateBook(Book book) => Update(book);
        public List<Book> UpdateBooks(List<Book> books) => UpdateMany(books);

        public Book FindByIsbn(string isbn) => _bookRepository.FindByIsbn(isbn);
        public Book FindByIsbn13(string isbn13) => _bookRepository.FindByIsbn13(isbn13);
        public Book FindByAsin(string asin) => _bookRepository.FindByAsin(asin);
        public Book FindByForeignId(string foreignBookId) => _bookRepository.FindByForeignId(foreignBookId);
        public Book FindByPath(string path) => _bookRepository.FindByPath(path);
        public List<Book> FindByAuthorId(int authorId) => _bookRepository.FindByAuthorId(authorId);
        public List<Book> FindBySeriesId(int seriesId) => _bookRepository.FindBySeriesId(seriesId);
        public Dictionary<int, string> AllBookPaths() => _bookRepository.AllBookPaths();
        public List<Book> GetBooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
            => _bookRepository.BooksBetweenDates(start, end, includeUnmonitored);
        public Dictionary<int, List<int>> AllBookTags() => _bookRepository.AllBookTags();
        public bool BookPathExists(string folder) => _bookRepository.BookPathExists(folder);

        protected override void OnItemAdded(Book item)
            => _eventAggregator.PublishEvent(new BookAddedEvent(item));

        protected override void OnItemsImported(List<Book> items)
            => _eventAggregator.PublishEvent(new BooksImportedEvent(items));

        protected override void OnItemDeleted(Book item, bool deleteFiles)
            => _eventAggregator.PublishEvent(new BookDeletedEvent(item, deleteFiles));

        protected override void OnItemsDeleted(List<Book> items, bool deleteFiles)
            => _eventAggregator.PublishEvent(new BooksDeletedEvent(items, deleteFiles));

        protected override void OnItemEdited(Book updated, Book stored)
            => _eventAggregator.PublishEvent(new BookEditedEvent(updated, stored));

        protected override void OnItemsBulkEdited(List<Book> items)
            => _eventAggregator.PublishEvent(new BooksBulkEditedEvent(items));
    }
}
