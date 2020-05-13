using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Artist;
using Readarr.Http;

namespace Readarr.Api.V1.Albums
{
    public abstract class AlbumModuleWithSignalR : ReadarrRestModuleWithSignalR<AlbumResource, Book>
    {
        protected readonly IBookService _bookService;
        protected readonly IAuthorStatisticsService _artistStatisticsService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;
        protected readonly IMapCoversToLocal _coverMapper;

        protected AlbumModuleWithSignalR(IBookService bookService,
                                           IAuthorStatisticsService artistStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _bookService = bookService;
            _artistStatisticsService = artistStatisticsService;
            _coverMapper = coverMapper;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumModuleWithSignalR(IBookService bookService,
                                           IAuthorStatisticsService artistStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _bookService = bookService;
            _artistStatisticsService = artistStatisticsService;
            _coverMapper = coverMapper;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetAlbum;
        }

        protected AlbumResource GetAlbum(int id)
        {
            var album = _bookService.GetBook(id);
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
                var artistDict = new Dictionary<int, NzbDrone.Core.Books.Author>();
                for (var i = 0; i < albums.Count; i++)
                {
                    var album = albums[i];
                    var resource = result[i];
                    var artist = artistDict.GetValueOrDefault(albums[i].AuthorMetadataId) ?? album.Author?.Value;
                    artistDict[artist.AuthorMetadataId] = artist;

                    resource.Artist = artist.ToResource();
                }
            }

            var artistStats = _artistStatisticsService.AuthorStatistics();
            LinkArtistStatistics(result, artistStats);
            MapCoversToLocal(result.ToArray());

            return result;
        }

        private void FetchAndLinkAlbumStatistics(AlbumResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.AuthorStatistics(resource.AuthorId));
        }

        private void LinkArtistStatistics(List<AlbumResource> resources, List<AuthorStatistics> artistStatistics)
        {
            foreach (var album in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.AuthorId == album.AuthorId);
                LinkArtistStatistics(album, stats);
            }
        }

        private void LinkArtistStatistics(AlbumResource resource, AuthorStatistics artistStatistics)
        {
            if (artistStatistics?.BookStatistics != null)
            {
                var dictAlbumStats = artistStatistics.BookStatistics.ToDictionary(v => v.BookId);

                resource.Statistics = dictAlbumStats.GetValueOrDefault(resource.Id).ToResource();
            }
        }

        private void MapCoversToLocal(params AlbumResource[] albums)
        {
            foreach (var albumResource in albums)
            {
                _coverMapper.ConvertToLocalUrls(albumResource.Id, MediaCoverEntity.Book, albumResource.Images);
            }
        }
    }
}
