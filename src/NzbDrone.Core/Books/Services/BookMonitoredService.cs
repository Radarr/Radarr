using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Books
{
    public interface IBookMonitoredService
    {
        void SetBookMonitoredStatus(Author author, MonitoringOptions monitoringOptions);
    }

    public class BookMonitoredService : IBookMonitoredService
    {
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly Logger _logger;

        public BookMonitoredService(IAuthorService authorService, IBookService bookService, Logger logger)
        {
            _authorService = authorService;
            _bookService = bookService;
            _logger = logger;
        }

        public void SetBookMonitoredStatus(Author author, MonitoringOptions monitoringOptions)
        {
            if (monitoringOptions != null)
            {
                _logger.Debug("[{0}] Setting book monitored status.", author.Name);

                var books = _bookService.GetBooksByAuthor(author.Id);

                var booksWithFiles = _bookService.GetAuthorBooksWithFiles(author);

                var booksWithoutFiles = books.Where(c => !booksWithFiles.Select(e => e.Id).Contains(c.Id) && c.ReleaseDate <= DateTime.UtcNow).ToList();

                var monitoredBooks = monitoringOptions.BooksToMonitor;

                // If specific books are passed use those instead of the monitoring options.
                if (monitoredBooks.Any())
                {
                    ToggleBooksMonitoredState(
                        books.Where(s => monitoredBooks.Any(t => t == s.ForeignBookId)), true);
                    ToggleBooksMonitoredState(
                        books.Where(s => monitoredBooks.Any(t => t != s.ForeignBookId)), false);
                }
                else
                {
                    switch (monitoringOptions.Monitor)
                    {
                        case MonitorTypes.All:
                            ToggleBooksMonitoredState(books, true);
                            break;
                        case MonitorTypes.Future:
                            _logger.Debug("Unmonitoring Books with Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithFiles.Select(c => c.Id).Contains(e.Id)), false);
                            _logger.Debug("Unmonitoring Books without Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithoutFiles.Select(c => c.Id).Contains(e.Id)), false);
                            break;
                        case MonitorTypes.None:
                            ToggleBooksMonitoredState(books, false);
                            break;
                        case MonitorTypes.Missing:
                            _logger.Debug("Unmonitoring Books with Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithFiles.Select(c => c.Id).Contains(e.Id)), false);
                            _logger.Debug("Monitoring Books without Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithoutFiles.Select(c => c.Id).Contains(e.Id)), true);
                            break;
                        case MonitorTypes.Existing:
                            _logger.Debug("Monitoring Books with Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithFiles.Select(c => c.Id).Contains(e.Id)), true);
                            _logger.Debug("Unmonitoring Books without Files");
                            ToggleBooksMonitoredState(books.Where(e => booksWithoutFiles.Select(c => c.Id).Contains(e.Id)), false);
                            break;
                        case MonitorTypes.Latest:
                            ToggleBooksMonitoredState(books, false);
                            ToggleBooksMonitoredState(books.OrderByDescending(e => e.ReleaseDate).Take(1), true);
                            break;
                        case MonitorTypes.First:
                            ToggleBooksMonitoredState(books, false);
                            ToggleBooksMonitoredState(books.OrderBy(e => e.ReleaseDate).Take(1), true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _bookService.UpdateMany(books);
            }

            _authorService.UpdateAuthor(author);
        }

        private void ToggleBooksMonitoredState(IEnumerable<Book> books, bool monitored)
        {
            foreach (var book in books)
            {
                book.Monitored = monitored;
            }
        }
    }
}
