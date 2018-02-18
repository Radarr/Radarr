using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Movies;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv;

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
            Put["/"] = Movie => SaveAll();
        }

        private Response SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var Movies = resources.Select(MovieResource => _movieSearch.MapMovieToTmdbMovie(MovieResource.ToModel())).Where(m => m != null).DistinctBy(m => m.TmdbId).ToList();

            return _movieService.AddMovies(Movies).ToResource().AsResponse(HttpStatusCode.Accepted);
        }
    }
}
