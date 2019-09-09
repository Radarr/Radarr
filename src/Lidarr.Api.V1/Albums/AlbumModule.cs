using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http.Extensions;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR,
        IHandle<AlbumGrabbedEvent>,
        IHandle<AlbumEditedEvent>,
        IHandle<AlbumImportedEvent>,
        IHandle<TrackImportedEvent>

    {
        protected readonly IReleaseService _releaseService;

        public AlbumModule(IAlbumService albumService,
                           IReleaseService releaseService,
                           IArtistStatisticsService artistStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IUpgradableSpecification upgradableSpecification,
                           IBroadcastSignalRMessage signalRBroadcaster)
        : base(albumService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
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
            var includeAllArtistAlbumsQuery = Request.Query.IncludeAllArtistAlbums;

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
                string foreignAlbumId = foreignIdQuery.Value.ToString();

                var album = _albumService.FindById(foreignAlbumId);

                if (includeAllArtistAlbumsQuery.HasValue && Convert.ToBoolean(includeAllArtistAlbumsQuery.Value))
                {
                    return MapToResource(_albumService.GetAlbumsByArtist(album.ArtistId), false);
                }
                else
                {
                    return MapToResource(new List<Album> { album }, false);
                }
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

        public void Handle(AlbumGrabbedEvent message)
        {
            foreach (var album in message.Album.Albums)
            {
                var resource = album.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }
        
        public void Handle(AlbumEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(AlbumImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.TrackInfo.Album.ToResource());
        }
    }
}
