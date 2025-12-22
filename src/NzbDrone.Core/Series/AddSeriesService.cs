using System.Collections.Generic;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.Series
{
    public interface IAddSeriesService
    {
        Series AddSeries(Series newSeries);
        List<Series> AddMultipleSeries(List<Series> newSeriesList, bool ignoreErrors = false);
    }

    public class AddSeriesService : IAddSeriesService
    {
        private readonly ISeriesService _seriesService;
        private readonly IAddSeriesValidator _addSeriesValidator;
        private readonly Logger _logger;

        public AddSeriesService(ISeriesService seriesService,
                                IAddSeriesValidator addSeriesValidator,
                                Logger logger)
        {
            _seriesService = seriesService;
            _addSeriesValidator = addSeriesValidator;
            _logger = logger;
        }

        public Series AddSeries(Series newSeries)
        {
            Ensure.That(newSeries, () => newSeries).IsNotNull();

            newSeries = SetPropertiesAndValidate(newSeries);

            _logger.Info("Adding Series {0}", newSeries);

            _seriesService.AddSeries(newSeries);

            return newSeries;
        }

        public List<Series> AddMultipleSeries(List<Series> newSeriesList, bool ignoreErrors = false)
        {
            var seriesToAdd = new List<Series>();

            foreach (var s in newSeriesList)
            {
                _logger.Info("Adding Series {0}", s);

                try
                {
                    var series = SetPropertiesAndValidate(s);
                    seriesToAdd.Add(series);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Series {0} was not added due to validation failures. {1}", s.Title, ex.Message);
                }
            }

            return _seriesService.AddMultipleSeries(seriesToAdd);
        }

        private Series SetPropertiesAndValidate(Series newSeries)
        {
            if (string.IsNullOrWhiteSpace(newSeries.SortTitle))
            {
                newSeries.SortTitle = newSeries.Title?.ToLowerInvariant();
            }

            var validationResult = _addSeriesValidator.Validate(newSeries);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newSeries;
        }
    }
}
