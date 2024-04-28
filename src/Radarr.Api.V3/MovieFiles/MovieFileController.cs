using System.Collections.Generic;
using System.Linq;
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
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;

        public MovieFileController(IBroadcastSignalRMessage signalRBroadcaster,
                               IMediaFileService mediaFileService,
                               IDeleteMediaFiles mediaFileDeletionService,
                               IMovieService movieService,
                               ICustomFormatCalculationService formatCalculator,
                               IUpgradableSpecification qualityUpgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _movieService = movieService;
            _formatCalculator = formatCalculator;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
        }

        protected override MovieFileResource GetResourceById(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);

            var resource = movieFile.ToResource(movie, _qualityUpgradableSpecification, _formatCalculator);

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
                    .ConvertAll(e => e.ToResource(_movieService.GetMovie(f.Key), _qualityUpgradableSpecification, _formatCalculator)))
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
                    // Don't allow user to set movieFile with 'Any' or 'Original' language
                    movieFile.Languages = resource.Languages.Where(l => l != Language.Any || l != Language.Original || l != null).ToList();
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
            return Accepted(movieFiles.ConvertAll(f => f.ToResource(movie, _qualityUpgradableSpecification, _formatCalculator)));
        }

        [RestDeleteById]
        public void DeleteMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);

            if (movieFile == null)
            {
                throw new NzbDroneClientException(global::System.Net.HttpStatusCode.NotFound, "Movie file not found");
            }

            var movie = _movieService.GetMovie(movieFile.MovieId);

            _mediaFileDeletionService.DeleteMovieFile(movie, movieFile);
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public object DeleteMovieFiles([FromBody] MovieFileListResource resource)
        {
            var movieFiles = _mediaFileService.GetMovies(resource.MovieFileIds);
            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            foreach (var movieFile in movieFiles)
            {
                _mediaFileDeletionService.DeleteMovieFile(movie, movieFile);
            }

            return new { };
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
