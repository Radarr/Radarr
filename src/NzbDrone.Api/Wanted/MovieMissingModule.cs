using NzbDrone.Api.Movie;
using NzbDrone.Api.Movies;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Datastore;
using NzbDrone.SignalR;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using System;
using NzbDrone.Core.Datastore.Events;

namespace NzbDrone.Api.Wanted
{
    class MovieMissingModule : NzbDroneRestModuleWithSignalR<MovieResource, Core.Tv.Movie>,
        IHandle<MovieGrabbedEvent>,
        IHandle<MovieDownloadedEvent>
    {
        protected readonly IMovieService _movieService;

        public MovieMissingModule(IMovieService movieService, 
                                  IQualityUpgradableSpecification qualityUpgradableSpecification, 
                                  IBroadcastSignalRMessage signalRBroadcaster) 
            : base(signalRBroadcaster, "wanted/missing")
        {

            _movieService = movieService;
            GetResourcePaged = GetMissingMovies;
        }

        private PagingResource<MovieResource> GetMissingMovies(PagingResource<MovieResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<MovieResource, Core.Tv.Movie>("physicalRelease", SortDirection.Descending);

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true;
            }

            var resource = ApplyToPage(_movieService.MoviesWithoutFiles, pagingSpec, v => MapToResource(v, false));

            return resource;
        }

        private MovieResource GetMovie(int id)
        {
            var movie = _movieService.GetMovie(id);
            var resource = MapToResource(movie, true);
            return resource;
        }

        private MovieResource MapToResource(Core.Tv.Movie movie, bool includeMovieFile)
        {
            var resource = movie.ToResource();
            return resource;
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var resource = message.Movie.Movie.ToResource();

            //add a grabbed field in MovieResource?
            //resource.Grabbed = true;

            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        public void Handle(MovieDownloadedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Movie.Movie.Id);
        }
    }
}
