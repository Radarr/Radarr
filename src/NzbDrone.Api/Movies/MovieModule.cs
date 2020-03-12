using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;

namespace NzbDrone.Api.Movies
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
        private const string TITLE_SLUG_ROUTE = "/titleslug/(?<slug>[^/]+)";

        protected readonly IMovieService _moviesService;
        private readonly IMapCoversToLocal _coverMapper;

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                           IMovieService moviesService,
                           IMapCoversToLocal coverMapper,
                           RootFolderValidator rootFolderValidator,
                           MappedNetworkDriveValidator mappedNetworkDriveValidator,
                           MoviePathValidator moviesPathValidator,
                           MovieExistsValidator moviesExistsValidator,
                           MovieAncestorValidator moviesAncestorValidator,
                           SystemFolderValidator systemFolderValidator,
                           ProfileExistsValidator profileExistsValidator)
        : base(signalRBroadcaster)
        {
            _moviesService = moviesService;

            _coverMapper = coverMapper;

            GetResourceAll = AllMovie;
            GetResourceById = GetMovie;

            CreateResource = AddMovie;
            UpdateResource = UpdateMovie;
            DeleteResource = DeleteMovie;

            SharedValidator.RuleFor(s => s.ProfileId).ValidId();

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(moviesPathValidator)
                           .SetValidator(moviesAncestorValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.ProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
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

        private List<MovieResource> AllMovie()
        {
            var moviesResources = _moviesService.GetAllMovies().ToResource();

            MapCoversToLocal(moviesResources.ToArray());

            return moviesResources;
        }

        private int AddMovie(MovieResource moviesResource)
        {
            var model = moviesResource.ToModel();

            return _moviesService.AddMovie(model).Id;
        }

        private void UpdateMovie(MovieResource moviesResource)
        {
            var model = moviesResource.ToModel(_moviesService.GetMovie(moviesResource.Id));

            _moviesService.UpdateMovie(model);

            BroadcastResourceChange(ModelAction.Updated, moviesResource);
        }

        private void DeleteMovie(int id)
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

            _moviesService.DeleteMovie(id, deleteFiles, addExclusion);
        }

        private void MapCoversToLocal(params MovieResource[] movies)
        {
            foreach (var moviesResource in movies)
            {
                _coverMapper.ConvertToLocalUrls(moviesResource.Id, moviesResource.Images);
            }
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
