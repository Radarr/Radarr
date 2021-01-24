using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Books;
using Readarr.Http;
using Readarr.Http.Extensions;
using Readarr.Http.REST;

namespace Readarr.Api.V1.History
{
    public class HistoryModule : ReadarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Get("/since", x => GetHistorySince());
            Get("/author", x => GetAuthorHistory());
            Post("/failed", x => MarkAsFailed());
        }

        protected HistoryResource MapToResource(NzbDrone.Core.History.History model, bool includeAuthor, bool includeBook)
        {
            var resource = model.ToResource();

            if (includeAuthor)
            {
                resource.Author = model.Author.ToResource();
            }

            if (includeBook)
            {
                resource.Book = model.Book.ToResource();
            }

            if (model.Author != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Author.QualityProfile.Value, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, NzbDrone.Core.History.History>("date", SortDirection.Descending);
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");
            var includeBook = Request.GetBooleanQueryParameter("includeBook");

            var eventTypeFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "eventType");
            var bookIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "bookId");
            var downloadIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "downloadId");

            if (eventTypeFilter != null)
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(eventTypeFilter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (bookIdFilter != null)
            {
                var bookId = Convert.ToInt32(bookIdFilter.Value);
                pagingSpec.FilterExpressions.Add(h => h.BookId == bookId);
            }

            if (downloadIdFilter != null)
            {
                var downloadId = downloadIdFilter.Value;
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            return ApplyToPage(_historyService.Paged, pagingSpec, h => MapToResource(h, includeAuthor, includeBook));
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
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");
            var includeBook = Request.GetBooleanQueryParameter("includeBook");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeAuthor, includeBook)).ToList();
        }

        private List<HistoryResource> GetAuthorHistory()
        {
            var queryAuthorId = Request.Query.AuthorId;
            var queryBookId = Request.Query.BookId;
            var queryEventType = Request.Query.EventType;

            if (!queryAuthorId.HasValue)
            {
                throw new BadRequestException("authorId is missing");
            }

            int authorId = Convert.ToInt32(queryAuthorId.Value);
            HistoryEventType? eventType = null;
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");
            var includeBook = Request.GetBooleanQueryParameter("includeBook");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            if (queryBookId.HasValue)
            {
                int bookId = Convert.ToInt32(queryBookId.Value);

                return _historyService.GetByBook(bookId, eventType).Select(h => MapToResource(h, includeAuthor, includeBook)).ToList();
            }

            return _historyService.GetByAuthor(authorId, eventType).Select(h => MapToResource(h, includeAuthor, includeBook)).ToList();
        }

        private object MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object();
        }
    }
}
