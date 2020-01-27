using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using NzbDrone.Api.Movies;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using Radarr.Http.Extensions;

namespace NzbDrone.Api.NetImport
{
    public class ListImportModule : NzbDroneApiModule
    {
        private readonly IMovieService _movieService;
        private readonly ISearchForNewMovie _movieSearch;

        public ListImportModule(IMovieService movieService, ISearchForNewMovie movieSearch)
            : base("/movie/import")
        {
            _movieService = movieService;
            _movieSearch = movieSearch;
            Put("/", movie => SaveAll());
        }

        private object SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var movies = resources.Select(movieResource => _movieSearch.MapMovieToTmdbMovie(movieResource.ToModel())).Where(m => m != null).DistinctBy(m => m.TmdbId).ToList();

            return ResponseWithCode(_movieService.AddMovies(movies).ToResource(), HttpStatusCode.Accepted);
        }
    }
}
