using System.Linq;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForMovies : IHousekeepingTask
    {
        private readonly IMovieRepository _movieRepository;

        public UpdateCleanTitleForMovies(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public void Clean()
        {
            var movies = _movieRepository.All().ToList();

            movies.ForEach(s =>
            {
                s.CleanTitle = s.CleanTitle.CleanSeriesTitle();
                _movieRepository.Update(s);
            });
        }
    }
}
