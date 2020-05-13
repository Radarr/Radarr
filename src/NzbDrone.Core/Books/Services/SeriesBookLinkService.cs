using System.Collections.Generic;

namespace NzbDrone.Core.Books
{
    public interface ISeriesBookLinkService
    {
        List<SeriesBookLink> GetLinksBySeries(int seriesId);
        void InsertMany(List<SeriesBookLink> model);
        void UpdateMany(List<SeriesBookLink> model);
        void DeleteMany(List<SeriesBookLink> model);
    }

    public class SeriesBookLinkService : ISeriesBookLinkService
    {
        private readonly ISeriesBookLinkRepository _repo;

        public SeriesBookLinkService(ISeriesBookLinkRepository repo)
        {
            _repo = repo;
        }

        public List<SeriesBookLink> GetLinksBySeries(int seriesId)
        {
            return _repo.GetLinksBySeries(seriesId);
        }

        public void InsertMany(List<SeriesBookLink> model)
        {
            _repo.InsertMany(model);
        }

        public void UpdateMany(List<SeriesBookLink> model)
        {
            _repo.UpdateMany(model);
        }

        public void DeleteMany(List<SeriesBookLink> model)
        {
            _repo.DeleteMany(model);
        }
    }
}
