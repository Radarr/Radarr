using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
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
                                IHandle<MovieDeletedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IMovieService _moviesService;
        private readonly IAddMovieService _addMovieService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IMovieService moviesService,
                            IAddMovieService addMovieService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            MoviePathValidator moviesPathValidator,
                            MovieExistsValidator moviesExistsValidator,
                            MovieAncestorValidator moviesAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            ProfileExistsValidator profileExistsValidator,
                            MovieFolderAsRootFolderValidator movieFolderAsRootFolderValidator)
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _addMovieService = addMovieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
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
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .SetValidator(movieFolderAsRootFolderValidator)
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private List<MovieResource> AllMovie()
        {
            var tmdbId = Request.GetIntegerQueryParameter("tmdbId");
            var moviesResources = new List<MovieResource>();

            if (tmdbId > 0)
            {
                moviesResources.AddIfNotNull(_moviesService.FindByTmdbId(tmdbId).ToResource(_qualityUpgradableSpecification));
            }
            else
            {
                moviesResources.AddRange(_moviesService.GetAllMovies().ToResource(_qualityUpgradableSpecification));
            }

            MapCoversToLocal(moviesResources.ToArray());
            PopulateAlternateTitles(moviesResources);

            return moviesResources;
        }

        private MovieResource GetMovie(int id)
        {
            var movies = _moviesService.GetMovie(id);
            return MapToResource(movies);
        }

        protected MovieResource MapToResource(Movie movies)
        {
            if (movies == null)
            {
                return null;
            }

            var resource = movies.ToResource();
            MapCoversToLocal(resource);

            return resource;
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

            _moviesService.UpdateMovie(model);

            BroadcastResourceChange(ModelAction.Updated, moviesResource);
        }

        private void DeleteMovie(int id)
        {
            var addExclusion = Request.GetBooleanQueryParameter("addNetImportExclusion");
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");

            _moviesService.DeleteMovie(id, deleteFiles, addExclusion);
        }

        private void MapCoversToLocal(params MovieResource[] movies)
        {
            foreach (var moviesResource in movies)
            {
                _coverMapper.ConvertToLocalUrls(moviesResource.Id, moviesResource.Images);
            }
        }

        private void PopulateAlternateTitles(List<MovieResource> resources)
        {
            foreach (var resource in resources)
            {
                PopulateAlternateTitles(resource);
            }
        }

        private void PopulateAlternateTitles(MovieResource resource)
        {
            //var mappings = null;//_sceneMappingService.FindByTvdbId(resource.TvdbId);

            //if (mappings == null) return;

            //Not necessary anymore

            //resource.AlternateTitles = mappings.Select(v => new AlternateTitleResource { Title = v.Title, SeasonNumber = v.SeasonNumber, SceneSeasonNumber = v.SceneSeasonNumber }).ToList();
        }

        public void Handle(MovieImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedMovie.MovieId);
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
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Movie.ToResource());
        }

        public void Handle(MovieRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
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
