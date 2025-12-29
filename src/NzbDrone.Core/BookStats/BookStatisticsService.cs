using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.BookStats
{
    public interface IBookStatisticsService
    {
        List<BookStatistics> BookStatistics();
        BookStatistics BookStatistics(int bookId);
    }

    public class BookStatisticsService : IBookStatisticsService
    {
        private readonly IBookStatisticsRepository _bookStatisticsRepository;

        public BookStatisticsService(IBookStatisticsRepository bookStatisticsRepository)
        {
            _bookStatisticsRepository = bookStatisticsRepository;
        }

        public List<BookStatistics> BookStatistics()
        {
            var bookStatistics = _bookStatisticsRepository.BookStatistics();

            return bookStatistics.GroupBy(b => b.BookId).Select(b => b.First()).ToList();
        }

        public BookStatistics BookStatistics(int bookId)
        {
            var stats = _bookStatisticsRepository.BookStatistics(bookId);

            if (stats == null || stats.Count == 0)
            {
                return new BookStatistics();
            }

            return stats.First();
        }
    }
}
