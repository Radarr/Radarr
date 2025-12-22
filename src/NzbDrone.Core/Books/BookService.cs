using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface IBookService
    {
        Book GetBook(int bookId);
        List<Book> GetBooks(IEnumerable<int> bookIds);
        PagingSpec<Book> Paged(PagingSpec<Book> pagingSpec);
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

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IEventAggregator _eventAggregator;

        public BookService(IBookRepository bookRepository,
                           IEventAggregator eventAggregator)
        {
            _bookRepository = bookRepository;
            _eventAggregator = eventAggregator;
        }

        public Book GetBook(int bookId)
        {
            return _bookRepository.Get(bookId);
        }

        public List<Book> GetBooks(IEnumerable<int> bookIds)
        {
            return _bookRepository.Get(bookIds).ToList();
        }

        public PagingSpec<Book> Paged(PagingSpec<Book> pagingSpec)
        {
            return _bookRepository.GetPaged(pagingSpec);
        }

        public Book AddBook(Book newBook)
        {
            newBook.Added = DateTime.UtcNow;
            var book = _bookRepository.Insert(newBook);

            _eventAggregator.PublishEvent(new BookAddedEvent(GetBook(book.Id)));

            return book;
        }

        public List<Book> AddBooks(List<Book> newBooks)
        {
            var now = DateTime.UtcNow;
            foreach (var book in newBooks)
            {
                book.Added = now;
            }

            _bookRepository.InsertMany(newBooks);

            _eventAggregator.PublishEvent(new BooksImportedEvent(newBooks));

            return newBooks;
        }

        public Book FindByIsbn(string isbn)
        {
            return _bookRepository.FindByIsbn(isbn);
        }

        public Book FindByIsbn13(string isbn13)
        {
            return _bookRepository.FindByIsbn13(isbn13);
        }

        public Book FindByAsin(string asin)
        {
            return _bookRepository.FindByAsin(asin);
        }

        public Book FindByForeignId(string foreignBookId)
        {
            return _bookRepository.FindByForeignId(foreignBookId);
        }

        public Book FindByPath(string path)
        {
            return _bookRepository.FindByPath(path);
        }

        public List<Book> FindByAuthorId(int authorId)
        {
            return _bookRepository.FindByAuthorId(authorId);
        }

        public List<Book> FindBySeriesId(int seriesId)
        {
            return _bookRepository.FindBySeriesId(seriesId);
        }

        public Dictionary<int, string> AllBookPaths()
        {
            return _bookRepository.AllBookPaths();
        }

        public List<Book> GetBooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            return _bookRepository.BooksBetweenDates(start, end, includeUnmonitored);
        }

        public void DeleteBook(int bookId, bool deleteFiles)
        {
            var book = _bookRepository.Get(bookId);
            _bookRepository.Delete(bookId);
            _eventAggregator.PublishEvent(new BookDeletedEvent(book, deleteFiles));
        }

        public void DeleteBooks(List<int> bookIds, bool deleteFiles)
        {
            var books = _bookRepository.Get(bookIds).ToList();
            _bookRepository.DeleteMany(bookIds);
            _eventAggregator.PublishEvent(new BooksDeletedEvent(books, deleteFiles));
        }

        public List<Book> GetAllBooks()
        {
            return _bookRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllBookTags()
        {
            return _bookRepository.AllBookTags();
        }

        public Book UpdateBook(Book book)
        {
            var storedBook = GetBook(book.Id);
            var updatedBook = _bookRepository.Update(book);

            _eventAggregator.PublishEvent(new BookEditedEvent(updatedBook, storedBook));

            return updatedBook;
        }

        public List<Book> UpdateBooks(List<Book> books)
        {
            _bookRepository.UpdateMany(books);

            _eventAggregator.PublishEvent(new BooksBulkEditedEvent(books));

            return books;
        }

        public bool BookPathExists(string folder)
        {
            return _bookRepository.BookPathExists(folder);
        }
    }
}
