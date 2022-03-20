using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Credits
{
    public interface ICreditRepository : IBasicRepository<Credit>
    {
        List<Credit> FindByMovieMetadataId(int movieId);
        void DeleteForMovies(List<int> movieIds);
    }

    public class CreditRepository : BasicRepository<Credit>, ICreditRepository
    {
        public CreditRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Credit> FindByMovieMetadataId(int movieId)
        {
            return Query(x => x.MovieMetadataId == movieId);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieMetadataId));
        }
    }
}
