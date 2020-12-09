using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Nancy;
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
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Movies
{
    public class MovieModule : RadarrRestModuleWithSignalR<MovieResource, Movie>,
                                IHandle<MovieImportedEvent>,
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
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                           IMovieService moviesService,
                           IMovieTranslationService movieTranslationService,
                           IAddMovieService addMovieService,
                           IMapCoversToLocal coverMapper,
                           IManageCommandQueue commandQueueManager,
                           IUpgradableSpecification qualityUpgradableSpecification,
                           IConfigService configService,
                           RootFolderValidator rootFolderValidator,
                           MappedNetworkDriveValidator mappedNetworkDriveValidator,
                           MoviePathValidator moviesPathValidator,
                           MovieExistsValidator moviesExistsValidator,
                           MovieAncestorValidator moviesAncestorValidator,
                           RecycleBinValidator recycleBinValidator,
                           SystemFolderValidator systemFolderValidator,
                           ProfileExistsValidator profileExistsValidator,
                           MovieFolderAsRootFolderValidator movieFolderAsRootFolderValidator)
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _movieTranslationService = movieTranslationService;
            _addMovieService = addMovieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;

            GetResourceAll = AllMovie;
            GetResourceById = GetMovie;
            CreateResource = AddMovie;
            UpdateResource = UpdateMovie;
            DeleteResource = DeleteMovie;

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(moviesPathValidator)
                           .SetValidator(moviesAncestorValidator)
                           .SetValidator(recycleBinValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .SetValidator(movieFolderAsRootFolderValidator)
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty().When(s => s.TmdbId <= 0);
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private List<MovieResource> AllMovie()
        {
            var tmdbId = Request.GetIntegerQueryParameter("tmdbId");
            var moviesResources = new List<MovieResource>();

            Dictionary<string, FileInfo> coverFileInfos = null;

            if (tmdbId > 0)
            {
                var movie = _moviesService.FindByTmdbId(tmdbId);

                if (movie != null)
                {
                    moviesResources.AddIfNotNull(MapToResource(movie));
                }
            }
            else
            {
                var configLanguage = (Language)_configService.MovieInfoLanguage;
                var availDelay = _configService.AvailabilityDelay;
                var movieTask = Task.Run(() => _moviesService.GetAllMovies());

                var translations = _movieTranslationService
                    .GetAllTranslationsForLanguage(configLanguage)
                    .ToDictionary(x => x.MovieId);

                coverFileInfos = _coverMapper.GetCoverFileInfos();

                var movies = movieTask.GetAwaiter().GetResult();

                moviesResources = new List<MovieResource>(movies.Count);

                foreach (var movie in movies)
                {
                    var translation = GetTranslationFromDict(translations, movie, configLanguage);
                    var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);
                    _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images, coverFileInfos);
                    moviesResources.Add(resource);
                }
            }

            return moviesResources;
        }

        private MovieResource GetMovie(int id)
        {
            var movie = _moviesService.GetMovie(id);
            return MapToResource(movie);
        }

        protected MovieResource MapToResource(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            var availDelay = _configService.AvailabilityDelay;

            var translations = _movieTranslationService.GetAllTranslationsForMovie(movie.Id);
            var translation = GetMovieTranslation(translations, movie, (Language)_configService.MovieInfoLanguage);

            var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);
            MapCoversToLocal(resource);

            return resource;
        }

        private MovieTranslation GetMovieTranslation(List<MovieTranslation> translations, Movie movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == configLanguage && t.MovieId == movie.Id);
        }

        private MovieTranslation GetTranslationFromDict(Dictionary<int, MovieTranslation> translations, Movie movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            translations.TryGetValue(movie.Id, out var translation);
            return translation;
        }

        private int AddMovie(MovieResource moviesResource)
        {
            var movie = _addMovieService.AddMovie(moviesResource.ToModel());

            return movie.Id;
        }

        private void UpdateMovie(MovieResource moviesResource)
        {
            var moveFiles = Request.GetBooleanQueryParameter("moveFiles");
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
            var availDelay = _configService.AvailabilityDelay;

            var translations = _movieTranslationService.GetAllTranslationsForMovie(movie.Id);
            var translation = GetMovieTranslation(translations, movie, (Language)_configService.MovieInfoLanguage);

            BroadcastResourceChange(ModelAction.Updated, updatedMovie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
        }

        private void DeleteMovie(int id)
        {
            var addExclusion = Request.GetBooleanQueryParameter("addImportExclusion");
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");

            _moviesService.DeleteMovie(id, deleteFiles, addExclusion);
        }

        private void MapCoversToLocal(MovieResource movie)
        {
            _coverMapper.ConvertToLocalUrls(movie.Id, movie.Images);
        }

        public void Handle(MovieImportedEvent message)
        {
            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovie(message.ImportedMovie.Movie.Id);
            var translation = GetMovieTranslation(translations, message.ImportedMovie.Movie, (Language)_configService.MovieInfoLanguage);
            BroadcastResourceChange(ModelAction.Updated, message.ImportedMovie.Movie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.MovieId);
        }

        public void Handle(MovieUpdatedEvent message)
        {
            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovie(message.Movie.Id);
            var translation = GetMovieTranslation(translations, message.Movie, (Language)_configService.MovieInfoLanguage);
            BroadcastResourceChange(ModelAction.Updated, message.Movie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
        }

        public void Handle(MovieEditedEvent message)
        {
            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovie(message.Movie.Id);
            var translation = GetMovieTranslation(translations, message.Movie, (Language)_configService.MovieInfoLanguage);
            BroadcastResourceChange(ModelAction.Updated, message.Movie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
        }

        public void Handle(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                BroadcastResourceChange(ModelAction.Deleted, movie.Id);
            }
        }

        public void Handle(MovieRenamedEvent message)
        {
            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovie(message.Movie.Id);
            var translation = GetMovieTranslation(translations, message.Movie, (Language)_configService.MovieInfoLanguage);
            BroadcastResourceChange(ModelAction.Updated, message.Movie.ToResource(availDelay, translation, _qualityUpgradableSpecification));
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
            }
        }
    }
}
