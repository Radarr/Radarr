using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Radarr.Http.Extensions;
using Radarr.Http.REST;
using NzbDrone.Core.Movies;
using Radarr.Http.Mapping;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Messaging.Commands;

namespace Radarr.Api.V2.Movies
{
    public class MovieEditorModule : RadarrV2Module
    {
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;

        public MovieEditorModule(IMovieService movieService, IManageCommandQueue commandQueueManager)
            : base("/movie/editor")
        {
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            Put["/"] = movie => SaveAll();
            Delete["/"] = movie => DeleteMovies();
        }

        private Response SaveAll()
        {
            var resource = Request.Body.FromJson<MovieEditorResource>();
            var moviesToUpdate = _movieService.GetMovies(resource.MovieIds);
            var moviesToMove = new List<BulkMoveMovie>();

            foreach (var movie in moviesToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    movie.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    movie.ProfileId = resource.QualityProfileId.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    movie.RootFolderPath = resource.RootFolderPath;
                    moviesToMove.Add(new BulkMoveMovie
                    {
                        MovieId = movie.Id,
                        SourcePath = movie.Path
                    });
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => movie.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => movie.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            movie.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            if (resource.MoveFiles && moviesToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveMovieCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Movies = moviesToMove
                });
            }

            return _movieService.UpdateMovie(moviesToUpdate)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }

        private Response DeleteMovies()
        {
            var resource = Request.Body.FromJson<MovieEditorResource>();

            foreach (var id in resource.MovieIds)
            {
                _movieService.DeleteMovie(id, false, false);
            }

            return new object().AsResponse();
        }
    }
}
