using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Music;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR
    {
        public AlbumModule(IArtistService artistService,
                             IAlbumService albumService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistService, qualityUpgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetAlbums;
            UpdateResource = SetMonitored;
        }

        private List<AlbumResource> GetAlbums()
        {
            if (!Request.Query.ArtistId.HasValue)
            {
                throw new BadRequestException("artistId is missing");
            }

            var artistId = (int)Request.Query.ArtistId;

            var resources = MapToResource(_albumService.GetAlbumsByArtist(artistId), false);

            return resources;
        }

        private void SetMonitored(AlbumResource albumResource)
        {
            _albumService.SetAlbumMonitored(albumResource.Id, albumResource.Monitored);
        }
    }
}
