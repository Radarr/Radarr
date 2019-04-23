using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
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
		protected IMainDatabase _database;

        public AlternativeTitleRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
			_database = database;
        }

        public AlternativeTitle FindBySourceId(int sourceId)
        {
            return Query(q => q.Where(t => t.SourceId == sourceId).FirstOrDefault());
        }

        public List<AlternativeTitle> FindBySourceIds(List<int> sourceIds)
        {
            return Query(q => q.Where(t => t.SourceId.In(sourceIds)).ToList());
        }

        public List<AlternativeTitle> FindByMovieId(int movieId)
        {
            return Query(q => q.Where(t => t.MovieId == movieId).ToList());
        }
    }
}
