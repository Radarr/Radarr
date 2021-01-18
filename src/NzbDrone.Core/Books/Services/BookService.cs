using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Books
{
    public interface IBookService
    {
        Book GetBook(int bookId);
        List<Book> GetBooks(IEnumerable<int> bookIds);
        List<Book> GetBooksByAuthor(int authorId);
        List<Book> GetNextBooksByAuthorMetadataId(IEnumerable<int> authorMetadataIds);
        List<Book> GetLastBooksByAuthorMetadataId(IEnumerable<int> authorMetadataIds);
        List<Book> GetBooksByAuthorMetadataId(int authorMetadataId);
        List<Book> GetBooksForRefresh(int authorMetadataId, IEnumerable<string> foreignIds);
        List<Book> GetBooksByFileIds(IEnumerable<int> fileIds);
        Book AddBook(Book newAlbum, bool doRefresh = true);
        Book FindById(string foreignId);
        Book FindBySlug(string titleSlug);
        Book FindByTitle(int authorMetadataId, string title);
        Book FindByTitleInexact(int authorMetadataId, string title);
        List<Book> GetCandidates(int authorMetadataId, string title);
        void DeleteBook(int bookId, bool deleteFiles, bool addImportListExclusion = false);
        List<Book> GetAllBooks();
        Book UpdateBook(Book book);
        void SetBookMonitored(int bookId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        PagingSpec<Book> BooksWithoutFiles(PagingSpec<Book> pagingSpec);
        List<Book> BooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Book> AuthorBooksBetweenDates(Author author, DateTime start, DateTime end, bool includeUnmonitored);
        void InsertMany(List<Book> books);
        void UpdateMany(List<Book> books);
        void DeleteMany(List<Book> books);
        void SetAddOptions(IEnumerable<Book> books);
        List<Book> GetAuthorBooksWithFiles(Author author);
    }

    public class BookService : IBookService,
                                IHandle<AuthorDeletedEvent>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IEditionService _editionService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public BookService(IBookRepository bookRepository,
                           IEditionService editionService,
                           IEventAggregator eventAggregator,
                           Logger logger)
        {
            _bookRepository = bookRepository;
            _editionService = editionService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Book AddBook(Book newBook, bool doRefresh = true)
        {
            _bookRepository.Upsert(newBook);

            var editions = newBook.Editions.Value;
            editions.ForEach(x => x.BookId = newBook.Id);

            _editionService.InsertMany(editions.Where(x => x.Id == 0).ToList());
            _editionService.SetMonitored(editions.FirstOrDefault(x => x.Monitored) ?? editions.First());

            _eventAggregator.PublishEvent(new BookAddedEvent(GetBook(newBook.Id), doRefresh));

            return newBook;
        }

        public void DeleteBook(int bookId, bool deleteFiles, bool addImportListExclusion = false)
        {
            var book = _bookRepository.Get(bookId);
            book.Author.LazyLoad();
            _bookRepository.Delete(bookId);
            _eventAggregator.PublishEvent(new BookDeletedEvent(book, deleteFiles, addImportListExclusion));
        }

        public Book FindById(string foreignId)
        {
            return _bookRepository.FindById(foreignId);
        }

        public Book FindBySlug(string titleSlug)
        {
            return _bookRepository.FindBySlug(titleSlug);
        }

        public Book FindByTitle(int authorMetadataId, string title)
        {
            return _bookRepository.FindByTitle(authorMetadataId, title);
        }

        private List<Tuple<Func<Book, string, double>, string>> BookScoringFunctions(string title, string cleanTitle)
        {
            Func<Func<Book, string, double>, string, Tuple<Func<Book, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Book, string, double>, string>>
            {
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), cleanTitle),
                tc((a, t) => a.Title.FuzzyMatch(t), title),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveBracketsAndContents().CleanAuthorName()),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveAfterDash().CleanAuthorName()),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveBracketsAndContents().RemoveAfterDash().CleanAuthorName()),
                tc((a, t) => t.FuzzyContains(a.CleanTitle), cleanTitle),
                tc((a, t) => t.FuzzyContains(a.Title), title)
            };

            return scoringFunctions;
        }

        public Book FindByTitleInexact(int authorMetadataId, string title)
        {
            var books = GetBooksByAuthorMetadataId(authorMetadataId);

            foreach (var func in BookScoringFunctions(title, title.CleanAuthorName()))
            {
                var results = FindByStringInexact(books, func.Item1, func.Item2);
                if (results.Count == 1)
                {
                    return results[0];
                }
            }

            return null;
        }

        public List<Book> GetCandidates(int authorMetadataId, string title)
        {
            var books = GetBooksByAuthorMetadataId(authorMetadataId);
            var output = new List<Book>();

            foreach (var func in BookScoringFunctions(title, title.CleanAuthorName()))
            {
                output.AddRange(FindByStringInexact(books, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        private List<Book> FindByStringInexact(List<Book> books, Func<Book, string, double> scoreFunction, string title)
        {
            const double fuzzThreshold = 0.7;
            const double fuzzGap = 0.4;

            var sortedBooks = books.Select(s => new
            {
                MatchProb = scoreFunction(s, title),
                Album = s
            })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            return sortedBooks.TakeWhile((x, i) => i == 0 || sortedBooks[i - 1].MatchProb - x.MatchProb < fuzzGap)
                .TakeWhile((x, i) => x.MatchProb > fuzzThreshold || (i > 0 && sortedBooks[i - 1].MatchProb > fuzzThreshold))
                .Select(x => x.Album)
                .ToList();
        }

        public List<Book> GetAllBooks()
        {
            return _bookRepository.All().ToList();
        }

        public Book GetBook(int bookId)
        {
            return _bookRepository.Get(bookId);
        }

        public List<Book> GetBooks(IEnumerable<int> bookIds)
        {
            return _bookRepository.Get(bookIds).ToList();
        }

        public List<Book> GetBooksByAuthor(int authorId)
        {
            return _bookRepository.GetBooks(authorId).ToList();
        }

        public List<Book> GetNextBooksByAuthorMetadataId(IEnumerable<int> authorMetadataIds)
        {
            return _bookRepository.GetNextBooks(authorMetadataIds).ToList();
        }

        public List<Book> GetLastBooksByAuthorMetadataId(IEnumerable<int> authorMetadataIds)
        {
            return _bookRepository.GetLastBooks(authorMetadataIds).ToList();
        }

        public List<Book> GetBooksByAuthorMetadataId(int authorMetadataId)
        {
            return _bookRepository.GetBooksByAuthorMetadataId(authorMetadataId).ToList();
        }

        public List<Book> GetBooksForRefresh(int authorMetadataId, IEnumerable<string> foreignIds)
        {
            return _bookRepository.GetBooksForRefresh(authorMetadataId, foreignIds);
        }

        public List<Book> GetBooksByFileIds(IEnumerable<int> fileIds)
        {
            return _bookRepository.GetBooksByFileIds(fileIds);
        }

        public void SetAddOptions(IEnumerable<Book> books)
        {
            _bookRepository.SetFields(books.ToList(), s => s.AddOptions);
        }

        public PagingSpec<Book> BooksWithoutFiles(PagingSpec<Book> pagingSpec)
        {
            var bookResult = _bookRepository.BooksWithoutFiles(pagingSpec);

            return bookResult;
        }

        public List<Book> BooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var books = _bookRepository.BooksBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return books;
        }

        public List<Book> AuthorBooksBetweenDates(Author author, DateTime start, DateTime end, bool includeUnmonitored)
        {
            var books = _bookRepository.AuthorBooksBetweenDates(author, start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return books;
        }

        public List<Book> GetAuthorBooksWithFiles(Author author)
        {
            return _bookRepository.GetAuthorBooksWithFiles(author);
        }

        public void InsertMany(List<Book> books)
        {
            _bookRepository.InsertMany(books);
        }

        public void UpdateMany(List<Book> books)
        {
            _bookRepository.UpdateMany(books);
        }

        public void DeleteMany(List<Book> books)
        {
            _bookRepository.DeleteMany(books);

            foreach (var book in books)
            {
                _eventAggregator.PublishEvent(new BookDeletedEvent(book, false, false));
            }
        }

        public Book UpdateBook(Book book)
        {
            var storedBook = GetBook(book.Id);
            var updatedBook = _bookRepository.Update(book);

            _eventAggregator.PublishEvent(new BookEditedEvent(updatedBook, storedBook));

            return updatedBook;
        }

        public void SetBookMonitored(int bookId, bool monitored)
        {
            var book = _bookRepository.Get(bookId);
            _bookRepository.SetMonitoredFlat(book, monitored);

            // publish book edited event so author stats update
            _eventAggregator.PublishEvent(new BookEditedEvent(book, book));

            _logger.Debug("Monitored flag for Book:{0} was set to {1}", bookId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _bookRepository.SetMonitored(ids, monitored);

            // publish book edited event so author stats update
            foreach (var book in _bookRepository.Get(ids))
            {
                _eventAggregator.PublishEvent(new BookEditedEvent(book, book));
            }
        }

        public void Handle(AuthorDeletedEvent message)
        {
            var books = GetBooksByAuthorMetadataId(message.Author.AuthorMetadataId);
            DeleteMany(books);
        }
    }
}
