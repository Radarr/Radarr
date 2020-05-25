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
                                IHandle<MoviesDeletedEvent>,
                                IHandle<MovieUpdatedEvent>,
                                IHandle<MovieEditedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private const string TITLE_SLUG_ROUTE = "/titleslug/(?<slug>[^/]+)";

        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IMapCoversToLocal _coverMapper;

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                           IMovieService movieService,
                           IAddMovieService addMovieService,
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
            _movieService = movieService;
            _addMovieService = addMovieService;

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
            var movies = _movieService.GetMovie(id);
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
            var moviesResources = _movieService.GetAllMovies().ToResource();

            MapCoversToLocal(moviesResources.ToArray());

            return moviesResources;
        }

        private int AddMovie(MovieResource moviesResource)
        {
            var model = moviesResource.ToModel();

            return _addMovieService.AddMovie(model).Id;
        }

        private void UpdateMovie(MovieResource moviesResource)
        {
            var model = moviesResource.ToModel(_movieService.GetMovie(moviesResource.Id));

            _movieService.UpdateMovie(model);

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

            _movieService.DeleteMovie(id, deleteFiles, addExclusion);
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

        public void Handle(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                BroadcastResourceChange(ModelAction.Deleted, movie.Id);
            }
        }

        public void Handle(MovieUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }

        public void Handle(MovieEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
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
