using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using Lidarr.Api.V1.Artist;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Http;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V1.Albums
{
    public abstract class AlbumModuleWithSignalR : LidarrRestModuleWithSignalR<AlbumResource, Album>
    {
        protected readonly IAlbumService _albumService;
        protected readonly IArtistStatisticsService _artistStatisticsService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;
        protected readonly IMapCoversToLocal _coverMapper;

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistStatisticsService artistStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _albumService = albumService;
            _artistStatisticsService = artistStatisticsService;
            _coverMapper = coverMapper;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumModuleWithSignalR(IAlbumService albumService,
                                           IArtistStatisticsService artistStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _albumService = albumService;
            _artistStatisticsService = artistStatisticsService;
            _coverMapper = coverMapper;
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
                var artist = album.Artist.Value;

                resource.Artist = artist.ToResource();
            }

            FetchAndLinkAlbumStatistics(resource);
            MapCoversToLocal(resource);

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
                    var artist = artistDict.GetValueOrDefault(albums[i].ArtistMetadataId) ?? album.Artist?.Value;
                    artistDict[artist.ArtistMetadataId] = artist;

                    resource.Artist = artist.ToResource();
                }
            }
            
            var artistStats = _artistStatisticsService.ArtistStatistics();
            LinkArtistStatistics(result, artistStats);
            MapCoversToLocal(result.ToArray());

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
            if (artistStatistics?.AlbumStatistics != null)
            {
                var dictAlbumStats = artistStatistics.AlbumStatistics.ToDictionary(v => v.AlbumId);

                resource.Statistics = dictAlbumStats.GetValueOrDefault(resource.Id).ToResource();

            }
        }

        private void MapCoversToLocal(params AlbumResource[] albums)
        {
            foreach (var albumResource in albums)
            {
                _coverMapper.ConvertToLocalUrls(albumResource.Id, MediaCoverEntity.Album, albumResource.Images);
            }
        }
    }
}
