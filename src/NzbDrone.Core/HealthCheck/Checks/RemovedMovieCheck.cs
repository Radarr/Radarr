using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(MovieUpdatedEvent))]
    [CheckOn(typeof(MovieDeletedEvent), CheckOnCondition.FailedOnly)]
    public class RemovedSeriesCheck : HealthCheckBase, ICheckOnCondition<MovieUpdatedEvent>, ICheckOnCondition<MovieDeletedEvent>
    {
        private readonly IMovieService _movieService;

        public RemovedSeriesCheck(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public override HealthCheck Check()
        {
            var deletedMovie = _movieService.GetAllMovies().Where(v => v.Status == MovieStatusType.Deleted).ToList();

            if (deletedMovie.Empty())
            {
                return new HealthCheck(GetType());
            }

            var movieText = deletedMovie.Select(s => $"{s.Title} (tmdbid {s.TmdbId})").Join(", ");

            if (deletedMovie.Count == 1)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Movie {0} was removed from TMDb", movieText), "#movie-was-removed-from-tmdb");
            }

            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Movie {0} was removed from TMDb", movieText), "#movie-was-removed-from-tmdb");
        }

        public bool ShouldCheckOnEvent(MovieDeletedEvent message)
        {
            return message.Movie.Status == MovieStatusType.Deleted;
        }

        public bool ShouldCheckOnEvent(MovieUpdatedEvent message)
        {
            return message.Movie.Status == MovieStatusType.Deleted;
        }
    }
}
