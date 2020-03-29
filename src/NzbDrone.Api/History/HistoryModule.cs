using System;
using System.Linq;
using Nancy;
using NzbDrone.Api.Movies;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Radarr.Http;

namespace NzbDrone.Api.History
{
    public class HistoryModule : RadarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             IUpgradableSpecification qualityUpgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Post("/failed", x => MarkAsFailed());
        }

        protected HistoryResource MapToResource(MovieHistory model)
        {
            var resource = model.ToResource();
            resource.Movie = model.Movie.ToResource();

            if (model.Movie != null)
            {
                resource.QualityCutoffNotMet = _qualityUpgradableSpecification.QualityCutoffNotMet(model.Movie.Profile, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var movieId = Request.Query.MovieId;
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, MovieHistory>("date", SortDirection.Descending);
            var filter = pagingResource.Filters.FirstOrDefault();

            if (filter != null && filter.Key == "eventType")
            {
                var filterValue = (MovieHistoryEventType)Convert.ToInt32(filter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (movieId.HasValue)
            {
                int i = (int)movieId;
                pagingSpec.FilterExpressions.Add(h => h.MovieId == i);
            }

            return ApplyToPage(_historyService.Paged, pagingSpec, MapToResource);
        }

        private object MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object();
        }
    }
}
