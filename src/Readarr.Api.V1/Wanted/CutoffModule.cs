using System.Linq;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Readarr.Api.V1.Albums;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Wanted
{
    public class CutoffModule : AlbumModuleWithSignalR
    {
        private readonly IAlbumCutoffService _albumCutoffService;

        public CutoffModule(IAlbumCutoffService albumCutoffService,
                            IAlbumService albumService,
                            IArtistStatisticsService artistStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
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

            var resource = ApplyToPage(_albumCutoffService.AlbumsWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeArtist));

            return resource;
        }
    }
}
