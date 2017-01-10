using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Movie
{
    public class MovieEditorModule : NzbDroneApiModule
    {
        private readonly IMovieService _movieService;

        public MovieEditorModule(IMovieService movieService)
            : base("/movie/editor")
        {
            _movieService = movieService;
            Put["/"] = Movie => SaveAll();
        }

        private Response SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var Movie = resources.Select(MovieResource => MovieResource.ToModel(_movieService.GetMovie(MovieResource.Id))).ToList();

            return _movieService.UpdateMovie(Movie)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }
    }
}
