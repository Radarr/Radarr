using System.Linq;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Albums;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Wanted
{
    public class MissingModule : AlbumModuleWithSignalR
    {
        public MissingModule(IBookService bookService,
                             IAuthorStatisticsService artistStatisticsService,
                             IMapCoversToLocal coverMapper,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(bookService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
            GetResourcePaged = GetMissingAlbums;
        }

        private PagingResource<AlbumResource> GetMissingAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Book>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var monitoredFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (monitoredFilter != null && monitoredFilter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Author.Value.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Author.Value.Monitored == true);
            }

            var resource = ApplyToPage(_bookService.BooksWithoutFiles, pagingSpec, v => MapToResource(v, includeArtist));

            return resource;
        }
    }
}
