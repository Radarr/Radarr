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
        private readonly IAddMovieService _addMovieService;
        private readonly ISearchForNewMovie _movieSearch;

        public ListImportModule(IAddMovieService addMovieService, ISearchForNewMovie movieSearch)
            : base("/movie/import")
        {
            _addMovieService = addMovieService;
            _movieSearch = movieSearch;
            Put("/", movie => SaveAll());
        }

        private object SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var movies = resources.Select(movieResource => _movieSearch.MapMovieToTmdbMovie(movieResource.ToModel())).Where(m => m != null).DistinctBy(m => m.TmdbId).ToList();

            return ResponseWithCode(_addMovieService.AddMovies(movies).ToResource(), HttpStatusCode.Accepted);
        }
    }
}
