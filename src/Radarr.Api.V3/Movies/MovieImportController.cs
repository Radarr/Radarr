using System;
using System.Collections.Generic;
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

        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public object Import([FromBody] List<MovieResource> resource)
        {
            var newMovies = resource.ToModel();

            return _addMovieService.AddMovies(newMovies).ToResource(0);
        }
    }
}
