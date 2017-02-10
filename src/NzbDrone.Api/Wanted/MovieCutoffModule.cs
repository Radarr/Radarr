﻿using NzbDrone.Api.Movie;
using NzbDrone.Api.Movies;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Datastore;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Wanted
{
    public class MovieCutoffModule : MovieModuleWithSignalR
    {
        private readonly IMovieCutoffService _movieCutoffService;

        public MovieCutoffModule(IMovieCutoffService movieCutoffService,
                                 IMovieService movieService,
                                 IQualityUpgradableSpecification qualityUpgradableSpecification,
                                 IBroadcastSignalRMessage signalRBroadcaster)
            : base(movieService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/cutoff")
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

            var resource = ApplyToPage(_movieCutoffService.MoviesWhereCutoffUnmet, pagingSpec, v => MapToResource(v, true));

            return resource;
        }
    }
}