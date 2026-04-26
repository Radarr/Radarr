using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public override Results<Ok<MovieResource>, NotFound> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Ok<List<MovieResource>> Import([FromBody] List<MovieResource> resource)
        {
            var newMovies = resource.ToModel();

            return TypedResults.Ok(_addMovieService.AddMovies(newMovies).ToResource(0));
        }
    }
}
