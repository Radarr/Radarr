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
    class MovieMissingModule : MovieModuleWithSignalR
    {
        protected readonly IMovieService _movieService;

        public MovieMissingModule(IMovieService movieService, 
                                  IQualityUpgradableSpecification qualityUpgradableSpecification, 
                                  IBroadcastSignalRMessage signalRBroadcaster) 
            : base(movieService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/missing")
        {

            _movieService = movieService;
            GetResourcePaged = GetMissingMovies;
        }

        private PagingResource<MovieResource> GetMissingMovies(PagingResource<MovieResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<MovieResource, Core.Tv.Movie>("title", SortDirection.Descending);

            pagingSpec.FilterExpression = _movieService.ConstructFilterExpression(pagingResource.FilterKey, pagingResource.FilterValue);

            var resource = ApplyToPage(_movieService.MoviesWithoutFiles, pagingSpec, v => MapToResource(v, true));

            return resource;
        }
    }
}
