using System.Collections.Generic;

namespace NzbDrone.Core.Books
{
    public interface ISeriesService
    {
        Series FindById(string foreignSeriesId);
        List<Series> FindById(IEnumerable<string> foreignSeriesId);
        List<Series> GetByAuthorMetadataId(int authorMetadataId);
        List<Series> GetByAuthorId(int authorId);
        void Delete(int seriesId);
        void InsertMany(IList<Series> series);
        void UpdateMany(IList<Series> series);
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;

        public SeriesService(ISeriesRepository seriesRepository)
        {
            _seriesRepository = seriesRepository;
        }

        public Series FindById(string foreignSeriesId)
        {
            return _seriesRepository.FindById(foreignSeriesId);
        }

        public List<Series> FindById(IEnumerable<string> foreignSeriesId)
        {
            return _seriesRepository.FindById(foreignSeriesId);
        }

        public List<Series> GetByAuthorMetadataId(int authorMetadataId)
        {
            return _seriesRepository.GetByAuthorMetadataId(authorMetadataId);
        }

        public List<Series> GetByAuthorId(int authorId)
        {
            return _seriesRepository.GetByAuthorId(authorId);
        }

        public void Delete(int seriesId)
        {
            _seriesRepository.Delete(seriesId);
        }

        public void InsertMany(IList<Series> series)
        {
            _seriesRepository.InsertMany(series);
        }

        public void UpdateMany(IList<Series> series)
        {
            _seriesRepository.UpdateMany(series);
        }
    }
}
