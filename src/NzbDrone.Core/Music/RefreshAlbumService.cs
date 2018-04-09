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
        void RefreshAlbumInfo(List<Album> albums);
    }

    public class RefreshAlbumService : IRefreshAlbumService, IExecute<RefreshAlbumCommand>
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfAlbumShouldBeRefreshed _checkIfAlbumShouldBeRefreshed;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IProvideAlbumInfo albumInfo,
                                   IRefreshTrackService refreshTrackService,
                                   IEventAggregator eventAggregator,
                                   ICheckIfAlbumShouldBeRefreshed checkIfAlbumShouldBeRefreshed,
                                   Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _albumInfo = albumInfo;
            _refreshTrackService = refreshTrackService;
            _eventAggregator = eventAggregator;
            _checkIfAlbumShouldBeRefreshed = checkIfAlbumShouldBeRefreshed;
            _logger = logger;
        }

        public void RefreshAlbumInfo(List<Album> albums)
        {
            foreach (var album in albums)
            {
                if (_checkIfAlbumShouldBeRefreshed.ShouldRefresh(album))
                {
                    RefreshAlbumInfo(album);
                }
            }
        }

        public void RefreshAlbumInfo(Album album)
        {
            _logger.ProgressInfo("Updating Info for {0}", album.Title);

            Tuple<Album, List<Track>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId, album.CurrentRelease?.Id);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error(
                    "Album '{0}' (LidarrAPI {1}) was not found, it may have been removed from Metadata sources.",
                    album.Title, album.ForeignAlbumId);
                return;
            }

            var albumInfo = tuple.Item1;

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
            album.AlbumType = albumInfo.AlbumType;
            album.SecondaryTypes = albumInfo.SecondaryTypes;
            album.Genres = albumInfo.Genres;
            album.Media = albumInfo.Media;
            album.Label = albumInfo.Label;
            album.Images = albumInfo.Images;
            album.ReleaseDate = albumInfo.ReleaseDate;
            album.Duration = tuple.Item2.Sum(track => track.Duration);
            album.Releases = albumInfo.Releases;
            album.Ratings = albumInfo.Ratings;
            album.CurrentRelease = albumInfo.CurrentRelease;

            _refreshTrackService.RefreshTrackInfo(album, tuple.Item2);

            _albumService.UpdateMany(new List<Album>{album});

            _logger.Debug("Finished album refresh for {0}", album.Title);

        }

        public void Execute(RefreshAlbumCommand message)
        {
            if (message.AlbumId.HasValue)
            {
                var album = _albumService.GetAlbum(message.AlbumId.Value);
                var artist = _artistService.GetArtist(album.ArtistId);
                RefreshAlbumInfo(album);
                _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
            }

        }
    }
}

