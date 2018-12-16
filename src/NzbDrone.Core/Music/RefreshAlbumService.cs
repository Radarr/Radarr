using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using NzbDrone.Core.Organizer;
using System.Linq;
using System.Text;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music.Commands;



namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumService
    {
        void RefreshAlbumInfo(Album album);
        void RefreshAlbumInfo(List<Album> albums, bool forceAlbumRefresh);
    }

    public class RefreshAlbumService : IRefreshAlbumService, IExecute<RefreshAlbumCommand>
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IReleaseService _releaseService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfAlbumShouldBeRefreshed _checkIfAlbumShouldBeRefreshed;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IArtistMetadataRepository artistMetadataRepository,
                                   IReleaseService releaseService,
                                   IProvideAlbumInfo albumInfo,
                                   IRefreshTrackService refreshTrackService,
                                   IEventAggregator eventAggregator,
                                   ICheckIfAlbumShouldBeRefreshed checkIfAlbumShouldBeRefreshed,
                                   Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _artistMetadataRepository = artistMetadataRepository;
            _releaseService = releaseService;
            _albumInfo = albumInfo;
            _refreshTrackService = refreshTrackService;
            _eventAggregator = eventAggregator;
            _checkIfAlbumShouldBeRefreshed = checkIfAlbumShouldBeRefreshed;
            _logger = logger;
        }

        public void RefreshAlbumInfo(List<Album> albums, bool forceAlbumRefresh)
        {
            foreach (var album in albums)
            {
                if (forceAlbumRefresh || _checkIfAlbumShouldBeRefreshed.ShouldRefresh(album))
                {
                    RefreshAlbumInfo(album);
                }
            }
        }

        public void RefreshAlbumInfo(Album album)
        {
            _logger.ProgressInfo("Updating Info for {0}", album.Title);

            Tuple<string, Album, List<ArtistMetadata>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error(
                    "Album '{0}' (LidarrAPI {1}) was not found, it may have been removed from Metadata sources.",
                    album.Title, album.ForeignAlbumId);
                return;
            }

            _artistMetadataRepository.UpsertMany(tuple.Item3);

            var albumInfo = tuple.Item2;

            if (album.ForeignAlbumId != albumInfo.ForeignAlbumId)
            {
                _logger.Warn(
                    "Album '{0}' (Album {1}) was replaced with '{2}' (LidarrAPI {3}), because the original was a duplicate.",
                    album.Title, album.ForeignAlbumId, albumInfo.Title, albumInfo.ForeignAlbumId);
                album.ForeignAlbumId = albumInfo.ForeignAlbumId;
            }

            album.LastInfoSync = DateTime.UtcNow;
            album.CleanTitle = albumInfo.CleanTitle;
            album.Title = albumInfo.Title ?? "Unknown";
            album.Overview = albumInfo.Overview;
            album.Disambiguation = albumInfo.Disambiguation;
            album.AlbumType = albumInfo.AlbumType;
            album.SecondaryTypes = albumInfo.SecondaryTypes;
            album.Genres = albumInfo.Genres;
            album.Images = albumInfo.Images;
            album.Links = albumInfo.Links;
            album.ReleaseDate = albumInfo.ReleaseDate;
            album.Ratings = albumInfo.Ratings;
            album.AlbumReleases = new List<AlbumRelease>();

            var remoteReleases = albumInfo.AlbumReleases.Value.DistinctBy(m => m.ForeignReleaseId).ToList();
            var existingReleases = _releaseService.GetReleasesByAlbum(album.Id);

            var newReleaseList = new List<AlbumRelease>();
            var updateReleaseList = new List<AlbumRelease>();

            foreach (var release in remoteReleases)
            {
                release.AlbumId = album.Id;
                var releaseToRefresh = existingReleases.SingleOrDefault(r => r.ForeignReleaseId == release.ForeignReleaseId);

                if (releaseToRefresh != null)
                {
                    existingReleases.Remove(releaseToRefresh);
                    release.Id = releaseToRefresh.Id;
                    release.Monitored = releaseToRefresh.Monitored;
                    updateReleaseList.Add(release);
                }
                else
                {
                    release.Monitored = false;
                    newReleaseList.Add(release);
                }
                album.AlbumReleases.Value.Add(release);
            }

            _releaseService.InsertMany(newReleaseList);
            _releaseService.UpdateMany(updateReleaseList);
            _releaseService.DeleteMany(existingReleases);

            if (album.AlbumReleases.Value.Count(x => x.Monitored) == 0)
            {
                var toMonitor = album.AlbumReleases.Value.OrderByDescending(x => x.TrackCount).First();
                toMonitor.Monitored = true;
                _releaseService.UpdateMany(new List<AlbumRelease> { toMonitor });
            }

            _refreshTrackService.RefreshTrackInfo(album);
            _albumService.UpdateMany(new List<Album>{album});

            _logger.Debug("Finished album refresh for {0}", album.Title);

        }

        public void Execute(RefreshAlbumCommand message)
        {
            if (message.AlbumId.HasValue)
            {
                var album = _albumService.GetAlbum(message.AlbumId.Value);
                var artist = _artistService.GetArtistByMetadataId(album.ArtistMetadataId);
                RefreshAlbumInfo(album);
                _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
            }

        }
    }
}

