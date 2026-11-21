using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Versions
{
    public interface IMovieVersionRepository : IBasicRepository<MovieVersion>
    {
        List<MovieVersion> FindByMovieId(int movieId);
        List<MovieVersion> FindByMovieIds(List<int> movieIds);
        MovieVersion FindByPath(string path);
        void DeleteForMovies(List<int> movieIds);
    }

    public class MovieVersionRepository : BasicRepository<MovieVersion>, IMovieVersionRepository
    {
        public MovieVersionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MovieVersion> FindByMovieId(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public List<MovieVersion> FindByMovieIds(List<int> movieIds)
        {
            return Query(x => movieIds.Contains(x.MovieId));
        }

        public MovieVersion FindByPath(string path)
        {
            return All().FirstOrDefault(x => x.Path.IsNotNullOrWhiteSpace() && x.Path.PathEquals(path));
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }
    }
}
