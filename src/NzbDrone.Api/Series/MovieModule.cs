using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Validation;
using NzbDrone.SignalR;
using NzbDrone.Core.Datastore;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;

namespace NzbDrone.Api.Movie
{
    public class MovieModule : NzbDroneRestModuleWithSignalR<MovieResource, Core.Tv.Movie>, 
                                IHandle<EpisodeImportedEvent>, 
                                IHandle<EpisodeFileDeletedEvent>,
                                IHandle<MovieUpdatedEvent>,       
                                IHandle<MovieEditedEvent>,  
                                IHandle<MovieDeletedEvent>,
                                IHandle<MovieRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>

    {
        protected readonly IMovieService _moviesService;
        private readonly IMovieStatisticsService _moviesStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;

		private const string TITLE_SLUG_ROUTE = "/titleslug/(?<slug>[^/]+)";

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IMovieService moviesService,
                            IMovieStatisticsService moviesStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper,
                            RootFolderValidator rootFolderValidator,
                            MoviePathValidator moviesPathValidator,
                            MovieExistsValidator moviesExistsValidator,
                            DroneFactoryValidator droneFactoryValidator,
                            MovieAncestorValidator moviesAncestorValidator,
                            ProfileExistsValidator profileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _moviesStatisticsService = moviesStatisticsService;

            _coverMapper = coverMapper;

            GetResourceAll = AllMovie;
			GetResourcePaged = GetMoviePaged;
            GetResourceById = GetMovie;
            Get[TITLE_SLUG_ROUTE] = GetByTitleSlug; /*(options) => {
				return ReqResExtensions.AsResponse(GetByTitleSlug(options.slug), Nancy.HttpStatusCode.OK);
			};*/



            CreateResource = AddMovie;
            UpdateResource = UpdateMovie;
            DeleteResource = DeleteMovie;

            Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.ProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(moviesPathValidator)
                           .SetValidator(droneFactoryValidator)
                           .SetValidator(moviesAncestorValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.ProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TmdbId).NotNull().NotEmpty().SetValidator(moviesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        public MovieModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IMovieService moviesService,
                            IMovieStatisticsService moviesStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper,
                            string resource)
            : base(signalRBroadcaster, resource)
        {
            _moviesService = moviesService;
            _moviesStatisticsService = moviesStatisticsService;

            _coverMapper = coverMapper;

            GetResourceAll = AllMovie;
            GetResourceById = GetMovie;
            CreateResource = AddMovie;
            UpdateResource = UpdateMovie;
            DeleteResource = DeleteMovie;
        }

        private MovieResource GetMovie(int id)
        {
            var movies = _moviesService.GetMovie(id);
            return MapToResource(movies);
        }

		private PagingResource<MovieResource> GetMoviePaged(PagingResource<MovieResource> pagingResource)
		{
			var pagingSpec = pagingResource.MapToPagingSpec<MovieResource, Core.Tv.Movie>();

            pagingSpec.FilterExpression = _moviesService.ConstructFilterExpression(pagingResource.FilterKey, pagingResource.FilterValue, pagingResource.FilterType);

            return ApplyToPage(_moviesService.Paged, pagingSpec, MapToResource);
		}

        protected MovieResource MapToResource(Core.Tv.Movie movies)
        {
            if (movies == null) return null;

            var resource = movies.ToResource();
            MapCoversToLocal(resource);
            //FetchAndLinkMovieStatistics(resource);
            //PopulateAlternateTitles(resource);

            return resource;
        }

        private List<MovieResource> AllMovie()
        {
            var moviesStats = _moviesStatisticsService.MovieStatistics();
            var moviesResources = _moviesService.GetAllMovies().ToResource();

            MapCoversToLocal(moviesResources.ToArray());
            LinkMovieStatistics(moviesResources, moviesStats);
            PopulateAlternateTitles(moviesResources);

            return moviesResources;
        }

		private Response GetByTitleSlug(dynamic options)
		{
            var slug = "";
            try
            {
                slug = options.slug;
                // do stuff with x
            }
            catch (RuntimeBinderException)
            {
                return new NotFoundResponse();
            }

            try
            {
                return MapToResource(_moviesService.FindByTitleSlug(slug)).AsResponse(Nancy.HttpStatusCode.OK);
            }
            catch (ModelNotFoundException)
            {
                return new NotFoundResponse();
            }
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

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _moviesStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(List<MovieResource> resources, List<MovieStatistics> moviesStatistics)
        {
            var dictMovieStats = moviesStatistics.ToDictionary(v => v.MovieId);

            foreach (var movies in resources)
            {
                var stats = dictMovieStats.GetValueOrDefault(movies.Id);
                if (stats == null) continue;

                LinkMovieStatistics(movies, stats);
            }
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics moviesStatistics)
        {
            //resource.SizeOnDisk = 0;//TODO: incorporate movie statistics moviesStatistics.SizeOnDisk;
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

        public void Handle(EpisodeImportedEvent message)
        {
            //BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.MovieId);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            //BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.MovieId);
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
