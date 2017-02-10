using NzbDrone.Api.Movie;
using NzbDrone.Api.Movies;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Datastore;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Wanted
{
    public class MovieCutoffModule : NzbDroneRestModuleWithSignalR<MovieResource, Core.Tv.Movie>
    {
        private readonly IMovieCutoffService _movieCutoffService;

        public MovieCutoffModule(IMovieCutoffService movieCutoffService,
                                 IMovieService movieService,
                                 IQualityUpgradableSpecification qualityUpgradableSpecification,
                                 IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster, "wanted/cutoff")
        {
            _movieCutoffService = movieCutoffService;
            GetResourcePaged = GetCutoffUnmetMovies;
        }

        private PagingResource<MovieResource> GetCutoffUnmetMovies(PagingResource<MovieResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<MovieResource, Core.Tv.Movie>("title", SortDirection.Ascending);

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true;
            }

            var resource = ApplyToPage(_movieCutoffService.MoviesWhereCutoffUnmet, pagingSpec, v => MapToResource(v));

            return resource;
        }

        private MovieResource MapToResource(Core.Tv.Movie movie)
        {
            var resource = movie.ToResource();
            return resource;
        }
    }
}