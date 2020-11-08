using System.Linq;
using NzbDrone.Api.Movies;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Http;

namespace NzbDrone.Api.Wanted
{
    public class MovieMissingModule : MovieModuleWithSignalR
    {
        public MovieMissingModule(IMovieService movieService,
                                  IUpgradableSpecification qualityUpgradableSpecification,
                                  IBroadcastSignalRMessage signalRBroadcaster)
            : base(movieService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
            GetResourcePaged = GetMissingMovies;
        }

        private PagingResource<MovieResource> GetMissingMovies(PagingResource<MovieResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<MovieResource, Movie>("title", SortDirection.Descending);
            var monitoredFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (monitoredFilter != null && monitoredFilter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true);
            }

            var resource = ApplyToPage(_movieService.MoviesWithoutFiles, pagingSpec, v => MapToResource(v));

            return resource;
        }
    }
}
