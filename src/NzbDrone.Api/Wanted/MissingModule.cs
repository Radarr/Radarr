using NzbDrone.Api.Episodes;
using NzbDrone.Api.Albums;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Wanted
{
    public class MissingModule : AlbumModuleWithSignalR
    {
        public MissingModule(IAlbumService albumService,
                             IArtistService artistService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
            GetResourcePaged = GetMissingAlbums;
        }

        private PagingResource<AlbumResource> GetMissingAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<AlbumResource, Album>("releaseDate", SortDirection.Descending);

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Artist.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Artist.Monitored == true;
            }

            var resource = ApplyToPage(_albumService.AlbumsWithoutFiles, pagingSpec, v => MapToResource(v, true));

            return resource;
        }
    }
}
