using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Api.TrackFiles;
using NzbDrone.Api.Music;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Albums
{
    public abstract class AlbumModuleWithSignalR : NzbDroneRestModuleWithSignalR<AlbumResource, Track>
    {
        protected readonly IAlbumService _albumService;
        protected readonly IArtistStatisticsService _artistStatisticsService;
        protected readonly IArtistService _artistService;
        protected readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistStatisticsService artistStatisticsService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _albumService = albumService;
            _artistStatisticsService = artistStatisticsService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistStatisticsService artistStatisticsService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _albumService = albumService;
            _artistStatisticsService = artistStatisticsService;
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

            FetchAndLinkAlbumStatistics(resource);

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

            for (var i = 0; i < albums.Count; i++)
            {
                var resource = result[i];
                FetchAndLinkAlbumStatistics(resource);
            }


            return result;
        }

        private void FetchAndLinkAlbumStatistics(AlbumResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.ArtistStatistics(resource.ArtistId));
        }

        private void LinkArtistStatistics(AlbumResource resource, ArtistStatistics artistStatistics)
        {
            if (artistStatistics.AlbumStatistics != null)
            {
                var dictAlbumStats = artistStatistics.AlbumStatistics.ToDictionary(v => v.AlbumId);

                resource.Statistics = dictAlbumStats.GetValueOrDefault(resource.Id).ToResource();
                
            }
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
