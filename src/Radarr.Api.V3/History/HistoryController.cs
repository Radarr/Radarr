using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Movies;
using Radarr.Api.V3.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.History
{
    [V3ApiController]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly IMovieService _movieService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryController(IHistoryService historyService,
                             IMovieService movieService,
                             ICustomFormatCalculationService formatCalculator,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _movieService = movieService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
        }

        protected HistoryResource MapToResource(MovieHistory model, bool includeMovie)
        {
            if (model.Movie == null)
            {
                model.Movie = _movieService.GetMovie(model.MovieId);
            }

            var resource = model.ToResource(_formatCalculator);

            if (includeMovie)
            {
                resource.Movie = model.Movie.ToResource(0);
            }

            if (model.Movie != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Movie.QualityProfile, model.Quality);
            }

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<HistoryResource> GetHistory([FromQuery] PagingRequestResource paging, bool includeMovie, int? eventType, string downloadId, [FromQuery] int[] movieIds = null, [FromQuery] int[] languages = null, [FromQuery] int[] quality = null)
        {
            var pagingResource = new PagingResource<HistoryResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, MovieHistory>("date", SortDirection.Descending);

            if (eventType.HasValue)
            {
                var filterValue = (MovieHistoryEventType)eventType.Value;
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            if (movieIds != null && movieIds.Any())
            {
                pagingSpec.FilterExpressions.Add(h => movieIds.Contains(h.MovieId));
            }

            return pagingSpec.ApplyToPage(h => _historyService.Paged(pagingSpec, languages, quality), h => MapToResource(h, includeMovie));
        }

        [HttpGet("since")]
        [Produces("application/json")]
        public List<HistoryResource> GetHistorySince(DateTime date, MovieHistoryEventType? eventType = null, bool includeMovie = false)
        {
            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeMovie)).ToList();
        }

        [HttpGet("movie")]
        [Produces("application/json")]
        public List<HistoryResource> GetMovieHistory(int movieId, MovieHistoryEventType? eventType = null, bool includeMovie = false)
        {
            return _historyService.GetByMovieId(movieId, eventType).Select(h => MapToResource(h, includeMovie)).ToList();
        }

        [HttpPost("failed/{id}")]
        public object MarkAsFailed([FromRoute] int id)
        {
            _failedDownloadService.MarkAsFailed(id);
            return new { };
        }
    }
}
