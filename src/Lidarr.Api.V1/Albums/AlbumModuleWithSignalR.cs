using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using Lidarr.Api.V1.Artist;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Http;

namespace Lidarr.Api.V1.Albums
{
    public abstract class AlbumModuleWithSignalR : LidarrRestModuleWithSignalR<AlbumResource, Album>
    {
        protected readonly IAlbumService _albumService;
        protected readonly IArtistStatisticsService _artistStatisticsService;
        protected readonly IArtistService _artistService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistStatisticsService artistStatisticsService,
                                           IArtistService artistService,
                                           IUpgradableSpecification qualityUpgradableSpecification,
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
                                           IUpgradableSpecification qualityUpgradableSpecification,
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
                var artistDict = new Dictionary<int, NzbDrone.Core.Music.Artist>();
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

            var artistList = albums.DistinctBy(a => a.ArtistId).ToList();
            var artistStats = _artistStatisticsService.ArtistStatistics();
            LinkArtistStatistics(result, artistStats);

            return result;
        }

        private void FetchAndLinkAlbumStatistics(AlbumResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.ArtistStatistics(resource.ArtistId));
        }

        private void LinkArtistStatistics(List<AlbumResource> resources, List<ArtistStatistics> artistStatistics)
        {
            foreach (var album in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.ArtistId == album.ArtistId);
                LinkArtistStatistics(album, stats);
            }
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
