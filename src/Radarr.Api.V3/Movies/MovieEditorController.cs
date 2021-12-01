using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("movie/editor")]
    public class MovieEditorController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public MovieEditorController(IMovieService movieService, IManageCommandQueue commandQueueManager, IUpgradableSpecification upgradableSpecification)
        {
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            _upgradableSpecification = upgradableSpecification;
        }

        [HttpPut]
        public IActionResult SaveAll([FromBody] MovieEditorResource resource)
        {
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

                if (resource.MinimumAvailability.HasValue)
                {
                    movie.MinimumAvailability = resource.MinimumAvailability.Value;
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

            return Accepted(_movieService.UpdateMovie(moviesToUpdate, !resource.MoveFiles).ToResource(0, _upgradableSpecification));
        }

        [HttpDelete]
        public object DeleteMovies([FromBody] MovieEditorResource resource)
        {
            _movieService.DeleteMovies(resource.MovieIds, resource.DeleteFiles, resource.AddImportExclusion);

            return new { };
        }
    }
}
