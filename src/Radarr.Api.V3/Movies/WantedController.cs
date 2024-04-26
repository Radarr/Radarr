using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("wanted")]
    public class WantedController : Controller
    {
        private readonly IMovieService _movieService;

        public WantedController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public MissingWantedResource GetWanted()
        {
            return _movieService.GetWantedMovies().ToResource();
        }
    }
}
