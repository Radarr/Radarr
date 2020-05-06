using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Readarr.Api.V1.Artist;
using Readarr.Http;

namespace Readarr.Api.V1.Albums
{
    public abstract class AlbumModuleWithSignalR : ReadarrRestModuleWithSignalR<AlbumResource, Book>
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

        protected AlbumResource MapToResource(Book album, bool includeArtist)
        {
            var resource = album.ToResource();

            if (includeArtist)
            {
                var artist = album.Author.Value;

                resource.Artist = artist.ToResource();
            }

            FetchAndLinkAlbumStatistics(resource);
            MapCoversToLocal(resource);

            return resource;
        }

        protected List<AlbumResource> MapToResource(List<Book> albums, bool includeArtist)
        {
            var result = albums.ToResource();

            if (includeArtist)
            {
                var artistDict = new Dictionary<int, NzbDrone.Core.Music.Author>();
                for (var i = 0; i < albums.Count; i++)
                {
                    var album = albums[i];
                    var resource = result[i];
                    var artist = artistDict.GetValueOrDefault(albums[i].AuthorMetadataId) ?? album.Author?.Value;
                    artistDict[artist.AuthorMetadataId] = artist;

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
            LinkArtistStatistics(resource, _artistStatisticsService.ArtistStatistics(resource.AuthorId));
        }

        private void LinkArtistStatistics(List<AlbumResource> resources, List<ArtistStatistics> artistStatistics)
        {
            foreach (var album in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.AuthorId == album.AuthorId);
                LinkArtistStatistics(album, stats);
            }
        }

        private void LinkArtistStatistics(AlbumResource resource, ArtistStatistics artistStatistics)
        {
            if (artistStatistics?.AlbumStatistics != null)
            {
                var dictAlbumStats = artistStatistics.AlbumStatistics.ToDictionary(v => v.BookId);

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
