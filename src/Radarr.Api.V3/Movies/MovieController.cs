using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController]
    public class MovieController : RestControllerWithSignalR<MovieResource, Movie>,
                                IHandle<MovieFileImportedEvent>,
                                IHandle<MovieFileDeletedEvent>,
                                IHandle<MovieUpdatedEvent>,
                                IHandle<MovieEditedEvent>,
                                IHandle<MoviesDeletedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IMovieService _moviesService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IAddMovieService _addMovieService;
        private readonly IMovieStatisticsService _movieStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MovieController(IBroadcastSignalRMessage signalRBroadcaster,
                           IMovieService moviesService,
                           IMovieTranslationService movieTranslationService,
                           IAddMovieService addMovieService,
                           IMovieStatisticsService movieStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IManageCommandQueue commandQueueManager,
                           IRootFolderService rootFolderService,
                           IUpgradableSpecification qualityUpgradableSpecification,
                           IConfigService configService,
                           RootFolderValidator rootFolderValidator,
                           MappedNetworkDriveValidator mappedNetworkDriveValidator,
                           MoviePathValidator moviesPathValidator,
                           MovieExistsValidator moviesExistsValidator,
                           MovieAncestorValidator moviesAncestorValidator,
                           RecycleBinValidator recycleBinValidator,
                           SystemFolderValidator systemFolderValidator,
                           QualityProfileExistsValidator qualityProfileExistsValidator,
                           RootFolderExistsValidator rootFolderExistsValidator,
                           MovieFolderAsRootFolderValidator movieFolderAsRootFolderValidator,
                           Logger logger)
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _movieTranslationService = movieTranslationService;
            _addMovieService = addMovieService;
            _movieStatisticsService = movieStatisticsService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;
            _logger = logger;

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.Stop)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(moviesPathValidator)
                           .SetValidator(moviesAncestorValidator)
                           .SetValidator(recycleBinValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .SetValidator(rootFolderExistsValidator)
                         .SetValidator(movieFolderAsRootFolderValidator)
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty().When(s => s.TmdbId <= 0);
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);
        }

        [HttpGet]
        public List<MovieResource> AllMovie(int? tmdbId, bool excludeLocalCovers = false, int? languageId = null)
        {
            var moviesResources = new List<MovieResource>();

            Dictionary<string, FileInfo> coverFileInfos = null;

            var translationLanguage = languageId is > 0
                ? Language.All.Single(l => l.Id == languageId.Value)
                : (Language)_configService.MovieInfoLanguage;

            if (tmdbId.HasValue)
            {
                var movie = _moviesService.FindByTmdbId(tmdbId.Value);

                if (movie != null)
                {
                    moviesResources.AddIfNotNull(MapToResource(movie, translationLanguage));
                }
            }
            else
            {
                var movieStats = _movieStatisticsService.MovieStatistics();
                var availDelay = _configService.AvailabilityDelay;

                var movieTask = Task.Run(() => _moviesService.GetAllMovies());

                var translations = _movieTranslationService
                    .GetAllTranslationsForLanguage(translationLanguage);

                var tdict = translations.ToDictionary(x => x.MovieMetadataId);
                var sdict = movieStats.ToDictionary(x => x.MovieId);

                if (!excludeLocalCovers)
                {
                    coverFileInfos = _coverMapper.GetCoverFileInfos();
                }

                var movies = movieTask.GetAwaiter().GetResult();

                moviesResources = new List<MovieResource>(movies.Count);

                foreach (var movie in movies)
                {
                    var translation = GetTranslationFromDict(tdict, movie.MovieMetadata, translationLanguage);
                    moviesResources.Add(movie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
                }

                if (!excludeLocalCovers)
                {
                    MapCoversToLocal(moviesResources, coverFileInfos);
                }

                LinkMovieStatistics(moviesResources, sdict);

                var rootFolders = _rootFolderService.All();

                moviesResources.ForEach(m => m.RootFolderPath = _rootFolderService.GetBestRootFolderPath(m.Path, rootFolders));
            }

            return moviesResources;
        }

        protected override MovieResource GetResourceById(int id)
        {
            var movie = _moviesService.GetMovie(id);

            return MapToResource(movie);
        }

        protected MovieResource MapToResource(Movie movie, Language translationLanguage = null)
        {
            if (movie == null)
            {
                return null;
            }

            translationLanguage ??= (Language)_configService.MovieInfoLanguage;
            var availDelay = _configService.AvailabilityDelay;

            var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId);
            var translation = GetMovieTranslation(translations, movie.MovieMetadata, translationLanguage);

            var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);
            MapCoversToLocal(resource);
            FetchAndLinkMovieStatistics(resource);

            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);

            return resource;
        }

        private MovieTranslation GetMovieTranslation(List<MovieTranslation> translations, MovieMetadata movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == configLanguage && t.MovieMetadataId == movie.Id);
        }

        private MovieTranslation GetTranslationFromDict(Dictionary<int, MovieTranslation> translations, MovieMetadata movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            if (!translations.TryGetValue(movie.Id, out var translation))
            {
                translation = new MovieTranslation
                {
                    Title = movie.Title,
                    Language = Language.English
                };
            }

            return translation;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<MovieResource> AddMovie([FromBody] MovieResource moviesResource)
        {
            var movie = _addMovieService.AddMovie(moviesResource.ToModel());

            return Created(movie.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<MovieResource> UpdateMovie([FromBody] MovieResource moviesResource, [FromQuery] bool moveFiles = false)
        {
            var movie = _moviesService.GetMovie(moviesResource.Id);

            if (moveFiles)
            {
                var sourcePath = movie.Path;
                var destinationPath = moviesResource.Path;

                _commandQueueManager.Push(new MoveMovieCommand
                {
                    MovieId = movie.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Trigger = CommandTrigger.Manual
                });
            }

            var model = moviesResource.ToModel(movie);

            var updatedMovie = _moviesService.UpdateMovie(model);

            BroadcastResourceChange(ModelAction.Updated, MapToResource(updatedMovie));

            return Accepted(moviesResource.Id);
        }

        [RestDeleteById]
        public void DeleteMovie(int id, bool deleteFiles = false, bool addImportExclusion = false)
        {
            _moviesService.DeleteMovie(id, deleteFiles, addImportExclusion);
        }

        private void MapCoversToLocal(MovieResource movie)
        {
            _coverMapper.ConvertToLocalUrls(movie.Id, movie.Images);
        }

        private void MapCoversToLocal(IEnumerable<MovieResource> movies, Dictionary<string, FileInfo> coverFileInfos)
        {
            _coverMapper.ConvertToLocalUrls(movies.Select(x => Tuple.Create(x.Id, x.Images.AsEnumerable())), coverFileInfos);
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _movieStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(List<MovieResource> resources, Dictionary<int, MovieStatistics> sDict)
        {
            foreach (var movie in resources)
            {
                if (sDict.TryGetValue(movie.Id, out var stats))
                {
                    LinkMovieStatistics(movie, stats);
                }
            }
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
            resource.HasFile = movieStatistics.MovieFileCount > 0;
            resource.SizeOnDisk = movieStatistics.SizeOnDisk;
        }

        [NonAction]
        public void Handle(MovieFileImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieInfo.Movie.Id);
        }

        [NonAction]
        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.MovieId);
        }

        [NonAction]
        public void Handle(MovieUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MovieEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                BroadcastResourceChange(ModelAction.Deleted, movie.Id);
            }
        }

        [NonAction]
        public void Handle(MovieRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Movie));
        }

        [NonAction]
        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
            }
        }
    }
}
