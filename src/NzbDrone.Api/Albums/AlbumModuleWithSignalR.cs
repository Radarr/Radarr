using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Api.TrackFiles;
using NzbDrone.Api.Music;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Albums
{
    public abstract class AlbumModuleWithSignalR : NzbDroneRestModuleWithSignalR<AlbumResource, Track>
    {
        protected readonly IAlbumService _albumService;
        protected readonly IArtistService _artistService;
        protected readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _albumService = albumService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _albumService = albumService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumResource GetAlbum(int id)
        {
            var album = _albumService.GetAlbum(id);
            var resource = MapToResource(album, true);
            return resource;
        }

        protected AlbumResource MapToResource(Album album, bool includeArtist)
        {
            var resource = album.ToResource();

            if (includeArtist)
            {
                var artist = album.Artist ?? _artistService.GetArtist(album.ArtistId);

                if (includeArtist)
                {
                    resource.Artist = artist.ToResource();
                }
            }

            return resource;
        }

        protected List<AlbumResource> MapToResource(List<Album> albums, bool includeArtist)
        {
            var result = albums.ToResource();

            if (includeArtist)
            {
                var artistDict = new Dictionary<int, Core.Music.Artist>();
                for (var i = 0; i < albums.Count; i++)
                {
                    var album = albums[i];
                    var resource = result[i];

                    var artist = album.Artist ?? artistDict.GetValueOrDefault(albums[i].ArtistId) ?? _artistService.GetArtist(albums[i].ArtistId);
                    artistDict[artist.Id] = artist;
                    
                    if (includeArtist)
                    {
                        resource.Artist = artist.ToResource();
                    }
                }
            }

            return result;
        }

        //TODO: Implement Track or Album Grabbed/Dowloaded Events

        //public void Handle(TrackGrabbedEvent message)
        //{
        //    foreach (var track in message.Track.Tracks)
        //    {
        //        var resource = track.ToResource();
        //        resource.Grabbed = true;

        //        BroadcastResourceChange(ModelAction.Updated, resource);
        //    }
        //}

        //public void Handle(TrackDownloadedEvent message)
        //{
        //    foreach (var album in message.Album.Albums)
        //    {
        //        BroadcastResourceChange(ModelAction.Updated, album.Id);
        //    }
        //}
    }
}
