using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Movies;
using Radarr.Api.V3.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.History
{
    public class HistoryModule : RadarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IMovieService _movieService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
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
            GetResourcePaged = GetHistory;

            Get("/since", x => GetHistorySince());
            Get("/movie", x => GetMovieHistory());
            Post("/failed", x => MarkAsFailed());
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
                resource.Movie = model.Movie.ToResource();
            }

            if (model.Movie != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Movie.Profile, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, MovieHistory>("date", SortDirection.Descending);
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            var eventTypeFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "eventType");
            var downloadIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "downloadId");

            if (eventTypeFilter != null)
            {
                var filterValue = (MovieHistoryEventType)Convert.ToInt32(eventTypeFilter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (downloadIdFilter != null)
            {
                var downloadId = downloadIdFilter.Value;
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            return ApplyToPage(_historyService.Paged, pagingSpec, h => MapToResource(h, includeMovie));
        }

        private List<HistoryResource> GetHistorySince()
        {
            var queryDate = Request.Query.Date;
            var queryEventType = Request.Query.EventType;

            if (!queryDate.HasValue)
            {
                throw new BadRequestException("date is missing");
            }

            DateTime date = DateTime.Parse(queryDate.Value);
            MovieHistoryEventType? eventType = null;
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            if (queryEventType.HasValue)
            {
                eventType = (MovieHistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeMovie)).ToList();
        }

        private List<HistoryResource> GetMovieHistory()
        {
            var queryMovieId = Request.Query.MovieId;
            var queryEventType = Request.Query.EventType;

            if (!queryMovieId.HasValue)
            {
                throw new BadRequestException("movieId is missing");
            }

            int movieId = Convert.ToInt32(queryMovieId.Value);
            MovieHistoryEventType? eventType = null;
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            if (queryEventType.HasValue)
            {
                eventType = (MovieHistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.GetByMovieId(movieId, eventType).Select(h => MapToResource(h, includeMovie)).ToList();
        }

        private object MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object();
        }
    }
}
