using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Api.V1.Albums;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V1.Wanted
{
    public class MissingModule : AlbumModuleWithSignalR
    {
        public MissingModule(IAlbumService albumService,
                             IArtistStatisticsService artistStatisticsService,
                             IMapCoversToLocal coverMapper,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
            GetResourcePaged = GetMissingAlbums;
        }

        private PagingResource<AlbumResource> GetMissingAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Album>
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
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Artist.Value.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Artist.Value.Monitored == true);
            }

            var resource = ApplyToPage(_albumService.AlbumsWithoutFiles, pagingSpec, v => MapToResource(v, includeArtist));

            return resource;
        }
    }
}
