using System;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Radarr.Api.V3.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Wanted
{
    [V3ApiController("wanted/missing")]
    public class MissingController : MovieControllerWithSignalR
    {
        public MissingController(IMovieService movieService,
                            IMovieTranslationService movieTranslationService,
                            IMovieStatisticsService movieStatisticsService,
                            IUpgradableSpecification upgradableSpecification,
                            ICustomFormatCalculationService formatCalculator,
                            IConfigService configService,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(movieService, movieTranslationService, movieStatisticsService, upgradableSpecification, formatCalculator, configService, signalRBroadcaster)
        {
        }

        [NonAction]
        public override ActionResult<MovieResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<MovieResource> GetMissingMovies([FromQuery] PagingRequestResource paging, bool monitored = true)
        {
            var pagingResource = new PagingResource<MovieResource>(paging);
            var pagingSpec = new PagingSpec<Movie>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            pagingSpec.FilterExpressions.Add(v => v.Monitored == monitored);

            var resource = pagingSpec.ApplyToPage(_movieService.MoviesWithoutFiles, v => MapToResource(v));

            return resource;
        }
    }
}
