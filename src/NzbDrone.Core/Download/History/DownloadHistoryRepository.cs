using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.History
{
    public interface IDownloadHistoryRepository : IBasicRepository<DownloadHistory>
    {
        List<DownloadHistory> FindByDownloadId(string downloadId);
        void DeleteByMovieIds(List<int> movieIds);
    }

    public class DownloadHistoryRepository : BasicRepository<DownloadHistory>, IDownloadHistoryRepository
    {
        public DownloadHistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<DownloadHistory> FindByDownloadId(string downloadId)
        {
            return Query(x => x.DownloadId == downloadId).OrderByDescending(h => h.Date).ToList();
        }

        public void DeleteByMovieIds(List<int> movieIds)
        {
            Delete(r => movieIds.Contains(r.MovieId));
        }
    }
}
