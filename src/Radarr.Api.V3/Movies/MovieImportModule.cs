using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Movies
{
    public class MovieImportModule : RadarrRestModule<MovieResource>
    {
        private readonly IAddMovieService _addMovieService;

        public MovieImportModule(IAddMovieService addMovieService)
            : base("/movie/import")
        {
            _addMovieService = addMovieService;
            Post("/", x => Import());
        }

        private object Import()
        {
            var resource = Request.Body.FromJson<List<MovieResource>>();
            var newMovies = resource.ToModel();

            return _addMovieService.AddMovies(newMovies).ToResource();
        }
    }
}
