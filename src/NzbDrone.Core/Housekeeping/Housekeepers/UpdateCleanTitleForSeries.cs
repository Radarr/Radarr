using System.Linq;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForSeries : IHousekeepingTask
    {
        private readonly IMovieRepository _movieRepository;

        public UpdateCleanTitleForSeries(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public void Clean()
        {
            var movies = _movieRepository.All().ToList();

            movies.ForEach(m =>
            {
                m.CleanTitle = m.CleanTitle.CleanSeriesTitle();
                _movieRepository.Update(m);
            });
        }
    }
}
