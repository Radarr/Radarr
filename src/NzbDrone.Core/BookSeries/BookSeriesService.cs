using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Monitoring;

namespace NzbDrone.Core.BookSeries
{
    public interface IBookSeriesService
    {
        BookSeries GetBookSeries(int bookSeriesId);
        List<BookSeries> GetBookSeriesItems(IEnumerable<int> bookSeriesIds);
        BookSeries AddBookSeries(BookSeries newBookSeries);
        List<BookSeries> AddMultipleBookSeries(List<BookSeries> newBookSeries);
        BookSeries FindByTitle(string title);
        BookSeries FindByForeignId(string foreignSeriesId);
        List<BookSeries> FindByAuthorId(int authorId);
        void DeleteBookSeries(int bookSeriesId);
        void DeleteMultipleBookSeries(List<int> bookSeriesIds);
        List<BookSeries> GetAllBookSeries();
        List<BookSeries> GetMonitoredBookSeries();
        BookSeries UpdateBookSeries(BookSeries bookSeries);
        List<BookSeries> UpdateMultipleBookSeries(List<BookSeries> bookSeries);
    }

    public class BookSeriesService : IBookSeriesService
    {
        private readonly IBookSeriesRepository _bookSeriesRepository;
        private readonly IHierarchicalMonitoringService _hierarchicalMonitoringService;

        public BookSeriesService(IBookSeriesRepository bookSeriesRepository,
                                 IHierarchicalMonitoringService hierarchicalMonitoringService)
        {
            _bookSeriesRepository = bookSeriesRepository;
            _hierarchicalMonitoringService = hierarchicalMonitoringService;
        }

        public BookSeries GetBookSeries(int bookSeriesId)
        {
            return _bookSeriesRepository.Get(bookSeriesId);
        }

        public List<BookSeries> GetBookSeriesItems(IEnumerable<int> bookSeriesIds)
        {
            return _bookSeriesRepository.Get(bookSeriesIds).ToList();
        }

        public BookSeries AddBookSeries(BookSeries newBookSeries)
        {
            return _bookSeriesRepository.Insert(newBookSeries);
        }

        public List<BookSeries> AddMultipleBookSeries(List<BookSeries> newBookSeries)
        {
            _bookSeriesRepository.InsertMany(newBookSeries);
            return newBookSeries;
        }

        public BookSeries FindByTitle(string title)
        {
            return _bookSeriesRepository.FindByTitle(title);
        }

        public BookSeries FindByForeignId(string foreignSeriesId)
        {
            return _bookSeriesRepository.FindByForeignId(foreignSeriesId);
        }

        public List<BookSeries> FindByAuthorId(int authorId)
        {
            return _bookSeriesRepository.FindByAuthorId(authorId);
        }

        public void DeleteBookSeries(int bookSeriesId)
        {
            _bookSeriesRepository.Delete(bookSeriesId);
        }

        public void DeleteMultipleBookSeries(List<int> bookSeriesIds)
        {
            _bookSeriesRepository.DeleteMany(bookSeriesIds);
        }

        public List<BookSeries> GetAllBookSeries()
        {
            return _bookSeriesRepository.All().ToList();
        }

        public List<BookSeries> GetMonitoredBookSeries()
        {
            return _bookSeriesRepository.GetMonitored();
        }

        public BookSeries UpdateBookSeries(BookSeries bookSeries)
        {
            var existingBookSeries = _bookSeriesRepository.Get(bookSeries.Id);

            if (existingBookSeries.Monitored != bookSeries.Monitored)
            {
                _hierarchicalMonitoringService.SetBookSeriesMonitored(bookSeries.Id, bookSeries.Monitored);
            }

            return _bookSeriesRepository.Update(bookSeries);
        }

        public List<BookSeries> UpdateMultipleBookSeries(List<BookSeries> bookSeries)
        {
            _bookSeriesRepository.UpdateMany(bookSeries);
            return bookSeries;
        }
    }
}
