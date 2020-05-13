using System;
using NLog;

namespace NzbDrone.Core.Books
{
    public interface ICheckIfBookShouldBeRefreshed
    {
        bool ShouldRefresh(Book book);
    }

    public class ShouldRefreshBook : ICheckIfBookShouldBeRefreshed
    {
        private readonly Logger _logger;

        public ShouldRefreshBook(Logger logger)
        {
            _logger = logger;
        }

        public bool ShouldRefresh(Book book)
        {
            if (book.LastInfoSync < DateTime.UtcNow.AddDays(-60))
            {
                _logger.Trace("Book {0} last updated more than 60 days ago, should refresh.", book.Title);
                return true;
            }

            if (book.LastInfoSync >= DateTime.UtcNow.AddHours(-12))
            {
                _logger.Trace("Book {0} last updated less than 12 hours ago, should not be refreshed.", book.Title);
                return false;
            }

            if (book.ReleaseDate > DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Book {0} released less than 30 days ago, should refresh.", book.Title);
                return true;
            }

            _logger.Trace("Book {0} released long ago and recently refreshed, should not be refreshed.", book.Title);
            return false;
        }
    }
}
