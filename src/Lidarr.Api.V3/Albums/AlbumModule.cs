using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;
using NzbDrone.Core.ArtistStats;

namespace Lidarr.Api.V3.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR
    {
        public AlbumModule(IArtistService artistService,
                             IAlbumService albumService,
                             IArtistStatisticsService artistStatisticsService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, artistService, upgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetAlbums;
            Put[@"/(?<id>[\d]{1,10})"] = x => SetAlbumMonitored(x.Id);
            Put["/monitor"] = x => SetAlbumsMonitored();
        }

        private List<AlbumResource> GetAlbums()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var albumIdsQuery = Request.Query.AlbumIds;

            if (!Request.Query.ArtistId.HasValue && !albumIdsQuery.HasValue)
            {
                return MapToResource(_albumService.GetAllAlbums(), false);
            }

            if (artistIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);

                return MapToResource(_albumService.GetAlbumsByArtist(artistId), false);
            }

            string albumIdsValue = albumIdsQuery.Value.ToString();

            var albumIds = albumIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_albumService.GetAlbums(albumIds), false);
        }

        private Response SetAlbumMonitored(int id)
        {
            var resource = Request.Body.FromJson<AlbumResource>();
            _albumService.SetAlbumMonitored(id, resource.Monitored);

            return MapToResource(_albumService.GetAlbum(id), false).AsResponse(HttpStatusCode.Accepted);
        }

        private Response SetAlbumsMonitored()
        {
            var resource = Request.Body.FromJson<AlbumsMonitoredResource>();

            _albumService.SetMonitored(resource.AlbumIds, resource.Monitored);

            return MapToResource(_albumService.GetAlbums(resource.AlbumIds), false).AsResponse(HttpStatusCode.Accepted);
        }
    }
}
