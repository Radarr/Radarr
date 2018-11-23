using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Radarr.Api.V2.Movies;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;

namespace Radarr.Api.V2.History
{
    public class HistoryModule : RadarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        // private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             // IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            // _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Get["/since"] = x => GetHistorySince();
            Get["/movie"] = x => GetMovieHistory();
            Post["/failed"] = x => MarkAsFailed();
        }

        protected HistoryResource MapToResource(NzbDrone.Core.History.History model, bool includeMovie)
        {
            var resource = model.ToResource();

            if (includeMovie)
            {
                resource.Movie = model.Movie.ToResource();
            }

            if (model.Movie != null)
            {
            //    resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Movie.Profile.Value, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, NzbDrone.Core.History.History>("date", SortDirection.Descending);
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            var eventTypeFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "eventType");

            if (eventTypeFilter != null)
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(eventTypeFilter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
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
            HistoryEventType? eventType = null;
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
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
            HistoryEventType? eventType = null;
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.GetByMovieId(movieId, eventType).Select(h => MapToResource(h, includeMovie)).ToList();
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object().AsResponse();
        }
    }
}
