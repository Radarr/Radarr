using System.Collections.Generic;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.BookSeries
{
    public interface IAddBookSeriesService
    {
        BookSeries AddBookSeries(BookSeries newBookSeries);
        List<BookSeries> AddMultipleBookSeries(List<BookSeries> newBookSeriesList, bool ignoreErrors = false);
    }

    public class AddBookSeriesService : IAddBookSeriesService
    {
        private readonly IBookSeriesService _bookSeriesService;
        private readonly IAddBookSeriesValidator _addBookSeriesValidator;
        private readonly Logger _logger;

        public AddBookSeriesService(IBookSeriesService bookSeriesService,
                                    IAddBookSeriesValidator addBookSeriesValidator,
                                    Logger logger)
        {
            _bookSeriesService = bookSeriesService;
            _addBookSeriesValidator = addBookSeriesValidator;
            _logger = logger;
        }

        public BookSeries AddBookSeries(BookSeries newBookSeries)
        {
            Ensure.That(newBookSeries, () => newBookSeries).IsNotNull();

            newBookSeries = SetPropertiesAndValidate(newBookSeries);

            _logger.Info("Adding BookSeries {0}", newBookSeries);

            _bookSeriesService.AddBookSeries(newBookSeries);

            return newBookSeries;
        }

        public List<BookSeries> AddMultipleBookSeries(List<BookSeries> newBookSeriesList, bool ignoreErrors = false)
        {
            var bookSeriesToAdd = new List<BookSeries>();

            foreach (var s in newBookSeriesList)
            {
                _logger.Info("Adding BookSeries {0}", s);

                try
                {
                    var bookSeries = SetPropertiesAndValidate(s);
                    bookSeriesToAdd.Add(bookSeries);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug(ex, "BookSeries {0} was not added due to validation failures.", s.Title);
                }
            }

            return _bookSeriesService.AddMultipleBookSeries(bookSeriesToAdd);
        }

        private BookSeries SetPropertiesAndValidate(BookSeries newBookSeries)
        {
            if (string.IsNullOrWhiteSpace(newBookSeries.SortTitle))
            {
                newBookSeries.SortTitle = newBookSeries.Title?.ToLowerInvariant();
            }

            var validationResult = _addBookSeriesValidator.Validate(newBookSeries);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newBookSeries;
        }
    }
}
