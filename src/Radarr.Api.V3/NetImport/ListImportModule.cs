using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using Radarr.Http.Extensions;
using Radarr.Api.V3.Movies;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;

namespace Radarr.Api.V3.NetImport
{
    public class ListImportModule : RadarrV3Module
    {
        private readonly IMovieService _movieService;
        private readonly ISearchForNewMovie _movieSearch;

        public ListImportModule(IMovieService movieService, ISearchForNewMovie movieSearch)
            : base("/movie/import")
        {
            _movieService = movieService;
            _movieSearch = movieSearch;
            Put("/",  Movie => SaveAll());
        }

        private object SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var Movies = resources.Select(MovieResource => _movieSearch.MapMovieToTmdbMovie(MovieResource.ToModel())).Where(m => m != null).DistinctBy(m => m.TmdbId).ToList();

            return ResponseWithCode(_movieService.AddMovies(Movies).ToResource(), HttpStatusCode.Accepted);
        }
    }
}
