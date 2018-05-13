using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieCutoffService
    {
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec);
    }

    public class MovieCutoffService : IMovieCutoffService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IProfileService _profileService;
        private readonly Logger _logger;

        public MovieCutoffService(IMovieRepository movieRepository, IProfileService profileService, Logger logger)
        {
            _movieRepository = movieRepository;
            _profileService = profileService;
            _logger = logger;
        }

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _profileService.All();

            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoffIndex = profile.Items.FindIndex(v => v.Quality.Id == profile.Cutoff.Id);
                var belowCutoff = profile.Items.Take(cutoffIndex).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.Select(i => i.Quality.Id)));
                }
            }

            return _movieRepository.MoviesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }
    }
}
