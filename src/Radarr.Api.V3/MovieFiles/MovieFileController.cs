using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using BadRequestException = Radarr.Http.REST.BadRequestException;

namespace Radarr.Api.V3.MovieFiles
{
    [V3ApiController]
    public class MovieFileController : RestControllerWithSignalR<MovieFileResource, MovieFile>,
                                 IHandle<MovieFileAddedEvent>,
                                 IHandle<MovieFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IMovieService _movieService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public MovieFileController(IBroadcastSignalRMessage signalRBroadcaster,
                               IMediaFileService mediaFileService,
                               IDeleteMediaFiles mediaFileDeletionService,
                               IMovieService movieService,
                               ICustomFormatCalculationService formatCalculator,
                               IUpgradableSpecification upgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _movieService = movieService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
        }

        protected override MovieFileResource GetResourceById(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);

            var resource = movieFile.ToResource(movie, _upgradableSpecification, _formatCalculator);

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<MovieFileResource> GetMovieFiles([FromQuery(Name = "movieId")] List<int> movieIds, [FromQuery] List<int> movieFileIds)
        {
            if (!movieIds.Any() && !movieFileIds.Any())
            {
                throw new BadRequestException("movieId or movieFileIds must be provided");
            }

            var movieFiles = movieIds.Any()
                ? _mediaFileService.GetFilesByMovies(movieIds)
                : _mediaFileService.GetMovies(movieFileIds);

            if (movieFiles == null)
            {
                return new List<MovieFileResource>();
            }

            return movieFiles.GroupBy(e => e.MovieId)
                .SelectMany(f => f.ToList()
                    .ConvertAll(e => e.ToResource(_movieService.GetMovie(f.Key), _upgradableSpecification, _formatCalculator)))
                .ToList();
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<MovieFileResource> SetMovieFile([FromBody] MovieFileResource movieFileResource)
        {
            var movieFile = _mediaFileService.GetMovie(movieFileResource.Id);
            movieFile.IndexerFlags = (IndexerFlags)movieFileResource.IndexerFlags;
            movieFile.Quality = movieFileResource.Quality;
            movieFile.Languages = movieFileResource.Languages;
            movieFile.Edition = movieFileResource.Edition;
            if (movieFileResource.ReleaseGroup != null)
            {
                movieFile.ReleaseGroup = movieFileResource.ReleaseGroup;
            }

            if (movieFileResource.SceneName != null && SceneChecker.IsSceneTitle(movieFileResource.SceneName))
            {
                movieFile.SceneName = movieFileResource.SceneName;
            }

            _mediaFileService.Update(movieFile);
            return Accepted(movieFile.Id);
        }

        [Obsolete("Use bulk endpoint instead")]
        [HttpPut("editor")]
        [Consumes("application/json")]
        public object SetMovieFile([FromBody] MovieFileListResource resource)
        {
            var movieFiles = _mediaFileService.GetMovies(resource.MovieFileIds);

            foreach (var movieFile in movieFiles)
            {
                if (resource.Quality != null)
                {
                    movieFile.Quality = resource.Quality;
                }

                if (resource.Languages != null)
                {
                    // Don't allow user to set files with 'Any' or 'Original' language
                    movieFile.Languages = resource.Languages.Where(l => l != null && l != Language.Any && l != Language.Original).ToList();
                }

                if (resource.IndexerFlags != null)
                {
                    movieFile.IndexerFlags = (IndexerFlags)resource.IndexerFlags.Value;
                }

                if (resource.Edition != null)
                {
                    movieFile.Edition = resource.Edition;
                }

                if (resource.ReleaseGroup != null)
                {
                    movieFile.ReleaseGroup = resource.ReleaseGroup;
                }

                if (resource.SceneName != null && SceneChecker.IsSceneTitle(resource.SceneName))
                {
                    movieFile.SceneName = resource.SceneName;
                }
            }

            _mediaFileService.Update(movieFiles);

            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            return Accepted(movieFiles.ConvertAll(f => f.ToResource(movie, _upgradableSpecification, _formatCalculator)));
        }

        [RestDeleteById]
        public void DeleteMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);

            if (movieFile == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Movie file not found");
            }

            var movie = _movieService.GetMovie(movieFile.MovieId);

            _mediaFileDeletionService.DeleteMovieFile(movie, movieFile);
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public object DeleteMovieFiles([FromBody] MovieFileListResource resource)
        {
            if (!resource.MovieFileIds.Any())
            {
                throw new BadRequestException("movieFileIds must be provided");
            }

            var movieFiles = _mediaFileService.GetMovies(resource.MovieFileIds);
            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            foreach (var movieFile in movieFiles)
            {
                _mediaFileDeletionService.DeleteMovieFile(movie, movieFile);
            }

            return new { };
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        public object SetPropertiesBulk([FromBody] List<MovieFileResource> resources)
        {
            var movieFiles = _mediaFileService.GetMovies(resources.Select(r => r.Id));

            foreach (var movieFile in movieFiles)
            {
                var resourceMovieFile = resources.Single(r => r.Id == movieFile.Id);

                if (resourceMovieFile.Languages != null)
                {
                    // Don't allow user to set files with 'Any' or 'Original' language
                    movieFile.Languages = resourceMovieFile.Languages.Where(l => l != null && l != Language.Any && l != Language.Original).ToList();
                }

                if (resourceMovieFile.Quality != null)
                {
                    movieFile.Quality = resourceMovieFile.Quality;
                }

                if (resourceMovieFile.SceneName != null && SceneChecker.IsSceneTitle(resourceMovieFile.SceneName))
                {
                    movieFile.SceneName = resourceMovieFile.SceneName;
                }

                if (resourceMovieFile.Edition != null)
                {
                    movieFile.Edition = resourceMovieFile.Edition;
                }

                if (resourceMovieFile.ReleaseGroup != null)
                {
                    movieFile.ReleaseGroup = resourceMovieFile.ReleaseGroup;
                }

                if (resourceMovieFile.IndexerFlags.HasValue)
                {
                    movieFile.IndexerFlags = (IndexerFlags)resourceMovieFile.IndexerFlags;
                }
            }

            _mediaFileService.Update(movieFiles);

            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            return Accepted(movieFiles.ConvertAll(f => f.ToResource(movie, _upgradableSpecification, _formatCalculator)));
        }

        [NonAction]
        public void Handle(MovieFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }

        [NonAction]
        public void Handle(MovieFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.MovieFile.Id);
        }
    }
}
