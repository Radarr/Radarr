using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Movies;
using Radarr.Http.Extensions;

namespace NzbDrone.Api.Movies
{
    public class MovieEditorModule : NzbDroneApiModule
    {
        private readonly IMovieService _movieService;

        public MovieEditorModule(IMovieService movieService)
            : base("/movie/editor")
        {
            _movieService = movieService;
            Put("/", movie => SaveAll());
            Put("/delete", movie => DeleteSelected());
        }

        private object SaveAll()
        {
            var resources = Request.Body.FromJson<List<MovieResource>>();

            var movie = resources.Select(movieResource => movieResource.ToModel(_movieService.GetMovie(movieResource.Id))).ToList();

            return ResponseWithCode(_movieService.UpdateMovie(movie)
                                    .ToResource(),
                                    HttpStatusCode.Accepted);
        }

        private object DeleteSelected()
        {
            var deleteFiles = false;
            var addExclusion = false;
            var deleteFilesQuery = Request.Query.deleteFiles;
            var addExclusionQuery = Request.Query.addExclusion;

            if (deleteFilesQuery.HasValue)
            {
                deleteFiles = Convert.ToBoolean(deleteFilesQuery.Value);
            }

            if (addExclusionQuery.HasValue)
            {
                addExclusion = Convert.ToBoolean(addExclusionQuery.Value);
            }

            var ids = Request.Body.FromJson<List<int>>();

            foreach (var id in ids)
            {
                _movieService.DeleteMovie(id, deleteFiles, addExclusion);
            }

            return new Response
            {
                StatusCode = HttpStatusCode.Accepted
            };
        }
    }
}
