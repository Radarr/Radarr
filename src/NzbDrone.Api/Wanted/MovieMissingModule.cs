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

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false;
            }
            else if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "true")
            {
                pagingSpec.FilterExpression = v => v.Monitored == true;
            }
            else if (pagingResource.FilterKey == "moviestatus"  && pagingResource.FilterValue == "available")
            {
                //TODO: might need to handle PreDB here
                pagingSpec.FilterExpression = v => v.Monitored == true &&
                             ((v.MinimumAvailability == MovieStatusType.Released && v.Status >= MovieStatusType.Released) ||
                             (v.MinimumAvailability == MovieStatusType.InCinemas && v.Status >= MovieStatusType.InCinemas) ||
                             (v.MinimumAvailability == MovieStatusType.Announced && v.Status >= MovieStatusType.Announced) ||
                             (v.MinimumAvailability == MovieStatusType.PreDB && v.Status >= MovieStatusType.Released));
            }
            else if (pagingResource.FilterKey == "moviestatus" && pagingResource.FilterValue == "announced")
            {
                pagingSpec.FilterExpression = v => v.Status == MovieStatusType.Announced;
            }
            else if (pagingResource.FilterKey == "moviestatus" && pagingResource.FilterValue == "incinemas")
            {
                pagingSpec.FilterExpression = v => v.Status == MovieStatusType.InCinemas;
            }
            else if (pagingResource.FilterKey == "moviestatus" && pagingResource.FilterValue == "released")
            {
                pagingSpec.FilterExpression = v => v.Status == MovieStatusType.Released;
            }

            var resource = ApplyToPage(_movieService.MoviesWithoutFiles, pagingSpec, v => MapToResource(v, true));

            return resource;
        }
    }
}
