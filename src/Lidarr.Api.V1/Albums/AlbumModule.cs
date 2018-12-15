using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;
using NzbDrone.Core.ArtistStats;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR
    {
        protected readonly IReleaseService _releaseService;
        
        public AlbumModule(IAlbumService albumService,
                           IReleaseService releaseService,
                           IArtistStatisticsService artistStatisticsService,
                           IUpgradableSpecification upgradableSpecification,
                           IBroadcastSignalRMessage signalRBroadcaster)
        : base(albumService, artistStatisticsService, upgradableSpecification, signalRBroadcaster)
        {
            _releaseService = releaseService;
            GetResourceAll = GetAlbums;
            UpdateResource = UpdateAlbum;
            Put["/monitor"] = x => SetAlbumsMonitored();
        }

        private List<AlbumResource> GetAlbums()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var albumIdsQuery = Request.Query.AlbumIds;
            var foreignIdQuery = Request.Query.ForeignAlbumId;

            if (!Request.Query.ArtistId.HasValue && !albumIdsQuery.HasValue && !foreignIdQuery.HasValue)
            {
                return MapToResource(_albumService.GetAllAlbums(), false);
            }

            if (artistIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);

                return MapToResource(_albumService.GetAlbumsByArtist(artistId), false);
            }

            if (foreignIdQuery.HasValue)
            {
                int artistId = _albumService.FindById(foreignIdQuery.Value).ArtistId;

                return MapToResource(_albumService.GetAlbumsByArtist(artistId), false);
            }

            string albumIdsValue = albumIdsQuery.Value.ToString();

            var albumIds = albumIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_albumService.GetAlbums(albumIds), false);
        }

        private void UpdateAlbum(AlbumResource albumResource)
        {
            var album = _albumService.GetAlbum(albumResource.Id);

            var model = albumResource.ToModel(album);

            _albumService.UpdateAlbum(model);
            _releaseService.UpdateMany(model.AlbumReleases.Value);

            BroadcastResourceChange(ModelAction.Updated, model.Id);
        }

        private Response SetAlbumsMonitored()
        {
            var resource = Request.Body.FromJson<AlbumsMonitoredResource>();

            _albumService.SetMonitored(resource.AlbumIds, resource.Monitored);

            return MapToResource(_albumService.GetAlbums(resource.AlbumIds), false).AsResponse(HttpStatusCode.Accepted);
        }
    }
}
