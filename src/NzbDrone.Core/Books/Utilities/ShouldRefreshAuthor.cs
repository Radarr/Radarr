using System;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Books
{
    public interface ICheckIfAuthorShouldBeRefreshed
    {
        bool ShouldRefresh(Author author);
    }

    public class ShouldRefreshAuthor : ICheckIfAuthorShouldBeRefreshed
    {
        private readonly IBookService _bookService;
        private readonly Logger _logger;

        public ShouldRefreshAuthor(IBookService bookService, Logger logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        public bool ShouldRefresh(Author author)
        {
            if (author.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Author {0} last updated more than 30 days ago, should refresh.", author.Name);
                return true;
            }

            if (author.LastInfoSync >= DateTime.UtcNow.AddHours(-12))
            {
                _logger.Trace("Author {0} last updated less than 12 hours ago, should not be refreshed.", author.Name);
                return false;
            }

            if (author.Metadata.Value.Status == AuthorStatusType.Continuing && author.LastInfoSync < DateTime.UtcNow.AddDays(-2))
            {
                _logger.Trace("Author {0} is continuing and has not been refreshed in 2 days, should refresh.", author.Name);
                return true;
            }

            var lastBook = _bookService.GetBooksByAuthor(author.Id).OrderByDescending(e => e.ReleaseDate).FirstOrDefault();

            if (lastBook != null && lastBook.ReleaseDate > DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Last book in {0} released less than 30 days ago, should refresh.", author.Name);
                return true;
            }

            _logger.Trace("Author {0} ended long ago, should not be refreshed.", author.Name);
            return false;
        }
    }
}
