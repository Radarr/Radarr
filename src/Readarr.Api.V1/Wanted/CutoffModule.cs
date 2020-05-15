using System.Linq;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Books;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Wanted
{
    public class CutoffModule : BookModuleWithSignalR
    {
        private readonly IBookCutoffService _bookCutoffService;

        public CutoffModule(IBookCutoffService bookCutoffService,
                            IBookService bookService,
                            IAuthorStatisticsService authorStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(bookService, authorStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _bookCutoffService = bookCutoffService;
            GetResourcePaged = GetCutoffUnmetBooks;
        }

        private PagingResource<BookResource> GetCutoffUnmetBooks(PagingResource<BookResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Book>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");
            var filter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (filter != null && filter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Author.Value.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Author.Value.Monitored == true);
            }

            var resource = ApplyToPage(_bookCutoffService.BooksWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeAuthor));

            return resource;
        }
    }
}
