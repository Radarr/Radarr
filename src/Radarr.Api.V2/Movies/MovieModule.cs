using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using Radarr.Http.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Core.Validation;
using NzbDrone.SignalR;
using Nancy;
using Radarr.Http;

namespace Radarr.Api.V2.Movies
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
        protected readonly IMovieService _moviesService;
        private readonly IMapCoversToLocal _coverMapper;

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IMovieService moviesService,
                            IMapCoversToLocal coverMapper,
                            RootFolderValidator rootFolderValidator,
                            MoviePathValidator moviesPathValidator,
                            MovieExistsValidator moviesExistsValidator,
                            MovieAncestorValidator moviesAncestorValidator,
                            ProfileExistsValidator profileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;

            _coverMapper = coverMapper;

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
                           .SetValidator(moviesPathValidator)
                           .SetValidator(moviesAncestorValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private List<MovieResource> AllMovie()
        {
            var moviesResources = _moviesService.GetAllMovies().ToResource();

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
            if (movies == null) return null;

            var resource = movies.ToResource();
            MapCoversToLocal(resource);

            return resource;
        }

        private int AddMovie(MovieResource moviesResource)
        {
            var movie = _moviesService.AddMovie(moviesResource.ToModel());

            return movie.Id;
        }

        private void UpdateMovie(MovieResource moviesResource)
        {
            var model = moviesResource.ToModel(_moviesService.GetMovie(moviesResource.Id));

            _moviesService.UpdateMovie(model);

            BroadcastResourceChange(ModelAction.Updated, moviesResource);
        }

        private void DeleteMovie(int id)
        {
            var addExclusion = false;
            var addExclusionQuery = Request.Query.addExclusion;

            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");

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
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

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
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Id);
        }
    }
}
