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
    public class CutoffModule : AlbumModuleWithSignalR
    {
        private readonly IBookCutoffService _albumCutoffService;

        public CutoffModule(IBookCutoffService albumCutoffService,
                            IBookService bookService,
                            IAuthorStatisticsService artistStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(bookService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _albumCutoffService = albumCutoffService;
            GetResourcePaged = GetCutoffUnmetAlbums;
        }

        private PagingResource<AlbumResource> GetCutoffUnmetAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Book>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var filter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (filter != null && filter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Author.Value.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Author.Value.Monitored == true);
            }

            var resource = ApplyToPage(_albumCutoffService.BooksWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeArtist));

            return resource;
        }
    }
}
