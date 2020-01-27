using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public interface IAlternativeTitleRepository : IBasicRepository<AlternativeTitle>
    {
        AlternativeTitle FindBySourceId(int sourceId);
        List<AlternativeTitle> FindBySourceIds(List<int> sourceIds);
        List<AlternativeTitle> FindByMovieId(int movieId);
    }

    public class AlternativeTitleRepository : BasicRepository<AlternativeTitle>, IAlternativeTitleRepository
    {
        public AlternativeTitleRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public AlternativeTitle FindBySourceId(int sourceId)
        {
            return Query(x => x.SourceId == sourceId).FirstOrDefault();
        }

        public List<AlternativeTitle> FindBySourceIds(List<int> sourceIds)
        {
            return Query(x => sourceIds.Contains(x.SourceId));
        }

        public List<AlternativeTitle> FindByMovieId(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }
    }
}
