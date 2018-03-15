using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Api.V1.Albums;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Wanted
{
    public class CutoffModule : AlbumModuleWithSignalR
    {
        private readonly IAlbumCutoffService _albumCutoffService;

        public CutoffModule(IAlbumCutoffService albumCutoffService,
                            IAlbumService albumService,
                            IArtistStatisticsService artistStatisticsService,
                            IArtistService artistService,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, artistService, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _albumCutoffService = albumCutoffService;
            GetResourcePaged = GetCutoffUnmetAlbums;
        }

        private PagingResource<AlbumResource> GetCutoffUnmetAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Album>
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
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Artist.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Artist.Monitored == true);
            }

            var resource = ApplyToPage(_albumCutoffService.AlbumsWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeArtist));

            return resource;
        }
    }
}
