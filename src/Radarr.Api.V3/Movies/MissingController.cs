using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("missing")]
    public class MissingController : Controller
    {
        private readonly IMovieService _movieService;

        public MissingController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public MissingWantedResource GetMissing()
        {
            return _movieService.GetMissingMovies().ToResource();
        }
    }
}
