using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V2.Movies
{
    public class MovieImportModule : RadarrRestModule<MovieResource>
    {
        private readonly IMovieService _movieService;

        public MovieImportModule(IMovieService movieService)
            : base("/movie/import")
        {
            _movieService = movieService;
            Post["/"] = x => Import();
        }


        private Response Import()
        {
            var resource = Request.Body.FromJson<List<MovieResource>>();
            var newSeries = resource.ToModel();

            return _movieService.AddMovies(newSeries).ToResource().AsResponse();
        }
    }
}
