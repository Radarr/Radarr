using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(MovieUpdatedEvent))]
    [CheckOn(typeof(MoviesDeletedEvent))]
    [CheckOn(typeof(MovieRefreshCompleteEvent))]
    public class RemovedMovieCheck : HealthCheckBase, ICheckOnCondition<MovieUpdatedEvent>, ICheckOnCondition<MoviesDeletedEvent>
    {
        private readonly IMovieService _movieService;

        public RemovedMovieCheck(IMovieService movieService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _movieService = movieService;
        }

        public override HealthCheck Check()
        {
            var deletedMovie = _movieService.GetAllMovies().Where(v => v.MovieMetadata.Value.Status == MovieStatusType.Deleted).ToList();

            if (deletedMovie.Empty())
            {
                return new HealthCheck(GetType());
            }

            var movieText = deletedMovie.Select(s => $"{s.Title} (tmdbid {s.TmdbId})").Join(", ");

            if (deletedMovie.Count == 1)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("RemovedMovieCheckSingleMessage", new Dictionary<string, object>
                    {
                        { "movie", movieText }
                    }),
                    "#movie-was-removed-from-tmdb");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                _localizationService.GetLocalizedString("RemovedMovieCheckMultipleMessage", new Dictionary<string, object>
                {
                    { "movies", movieText }
                }),
                "#movie-was-removed-from-tmdb");
        }

        public bool ShouldCheckOnEvent(MoviesDeletedEvent message)
        {
            return message.Movies.Any(m => m.MovieMetadata.Value.Status == MovieStatusType.Deleted);
        }

        public bool ShouldCheckOnEvent(MovieUpdatedEvent message)
        {
            return message.Movie.MovieMetadata.Value.Status == MovieStatusType.Deleted;
        }
    }
}
