using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Monitoring;

namespace NzbDrone.Core.Series
{
    public interface ISeriesService
    {
        Series GetSeries(int seriesId);
        List<Series> GetSeriesItems(IEnumerable<int> seriesIds);
        Series AddSeries(Series newSeries);
        List<Series> AddMultipleSeries(List<Series> newSeries);
        Series FindByTitle(string title);
        Series FindByForeignId(string foreignSeriesId);
        List<Series> FindByAuthorId(int authorId);
        void DeleteSeries(int seriesId);
        void DeleteMultipleSeries(List<int> seriesIds);
        List<Series> GetAllSeries();
        List<Series> GetMonitoredSeries();
        Series UpdateSeries(Series series);
        List<Series> UpdateMultipleSeries(List<Series> series);
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IHierarchicalMonitoringService _hierarchicalMonitoringService;

        public SeriesService(ISeriesRepository seriesRepository,
                             IHierarchicalMonitoringService hierarchicalMonitoringService)
        {
            _seriesRepository = seriesRepository;
            _hierarchicalMonitoringService = hierarchicalMonitoringService;
        }

        public Series GetSeries(int seriesId)
        {
            return _seriesRepository.Get(seriesId);
        }

        public List<Series> GetSeriesItems(IEnumerable<int> seriesIds)
        {
            return _seriesRepository.Get(seriesIds).ToList();
        }

        public Series AddSeries(Series newSeries)
        {
            return _seriesRepository.Insert(newSeries);
        }

        public List<Series> AddMultipleSeries(List<Series> newSeries)
        {
            _seriesRepository.InsertMany(newSeries);
            return newSeries;
        }

        public Series FindByTitle(string title)
        {
            return _seriesRepository.FindByTitle(title);
        }

        public Series FindByForeignId(string foreignSeriesId)
        {
            return _seriesRepository.FindByForeignId(foreignSeriesId);
        }

        public List<Series> FindByAuthorId(int authorId)
        {
            return _seriesRepository.FindByAuthorId(authorId);
        }

        public void DeleteSeries(int seriesId)
        {
            _seriesRepository.Delete(seriesId);
        }

        public void DeleteMultipleSeries(List<int> seriesIds)
        {
            _seriesRepository.DeleteMany(seriesIds);
        }

        public List<Series> GetAllSeries()
        {
            return _seriesRepository.All().ToList();
        }

        public List<Series> GetMonitoredSeries()
        {
            return _seriesRepository.GetMonitored();
        }

        public Series UpdateSeries(Series series)
        {
            var existingSeries = _seriesRepository.Get(series.Id);

            if (existingSeries.Monitored != series.Monitored)
            {
                _hierarchicalMonitoringService.SetSeriesMonitored(series.Id, series.Monitored);
            }

            return _seriesRepository.Update(series);
        }

        public List<Series> UpdateMultipleSeries(List<Series> series)
        {
            _seriesRepository.UpdateMany(series);
            return series;
        }
    }
}
