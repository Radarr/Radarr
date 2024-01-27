using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("movie/import")]
    public class MovieImportController : RestController<MovieResource>
    {
        private readonly IAddMovieService _addMovieService;

        public MovieImportController(IAddMovieService addMovieService)
        {
            _addMovieService = addMovieService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<List<MovieResource>> Import([FromBody] List<MovieResource> resource)
        {
            var newMovies = resource.ToModel();
            var addedMovies = await _addMovieService.AddMovies(newMovies);

            return addedMovies.ToResource(0);
        }
    }
}
