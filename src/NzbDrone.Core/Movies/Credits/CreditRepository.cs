using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Credits
{
    public interface ICreditRepository : IBasicRepository<Credit>
    {
        List<Credit> FindByMovieId(int movieId);
        void DeleteForMovies(List<int> movieIds);
    }

    public class CreditRepository : BasicRepository<Credit>, ICreditRepository
    {
        public CreditRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Credit> FindByMovieId(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }
    }
}
