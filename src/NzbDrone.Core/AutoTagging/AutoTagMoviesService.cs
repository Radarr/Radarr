using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.AutoTagging
{
    public class AutoTagMoviesService : IExecute<AutoTagMoviesCommand>
    {
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public AutoTagMoviesService(IMovieService movieService, Logger logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        public void Execute(AutoTagMoviesCommand message)
        {
            var moviesToUpdate = new List<Movie>();

            foreach (var movie in _movieService.GetAllMovies())
            {
                if (_movieService.UpdateTags(movie))
                {
                    moviesToUpdate.Add(movie);
                }
            }

            if (moviesToUpdate.Count == 0)
            {
                _logger.Debug("No movies had auto-tag changes");
                return;
            }

            _logger.Info("Updating auto-tags for {0} movies", moviesToUpdate.Count);
            _movieService.UpdateMovie(moviesToUpdate, true);
        }
    }
}
