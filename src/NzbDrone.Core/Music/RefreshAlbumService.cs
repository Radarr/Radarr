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
        void RefreshAlbumInfo(Artist artist, IEnumerable<Album> remoteAlbums);
        void RefreshAlbumInfo(Album album);
    }

    public class RefreshAlbumService : IRefreshAlbumService, IExecute<RefreshAlbumCommand>
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly ITrackService _trackService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IProvideAlbumInfo albumInfo,
                                   IRefreshTrackService refreshTrackService,
                                   ITrackService trackService,
                                   IBuildFileNames fileNameBuilder,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _albumInfo = albumInfo;
            _refreshTrackService = refreshTrackService;
            _trackService = trackService;
            _fileNameBuilder = fileNameBuilder;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshAlbumInfo(Album album)
        {
            _logger.ProgressInfo("Updating Info for {0}", album.Title);

            Tuple<Album, List<Track>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId, album.CurrentRelease.Id);
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
            album.CleanTitle = Parser.Parser.CleanArtistName(album.Title);
            album.AlbumType = albumInfo.AlbumType;
            album.SecondaryTypes = albumInfo.SecondaryTypes;
            album.Genres = albumInfo.Genres;
            album.Media = albumInfo.Media;
            album.Label = albumInfo.Label;
            album.Images = albumInfo.Images;
            album.ReleaseDate = albumInfo.ReleaseDate;
            album.Duration = tuple.Item2.Sum(track => track.Duration);
            album.Releases = albumInfo.Releases;

            _refreshTrackService.RefreshTrackInfo(album, tuple.Item2);

            _albumService.UpdateAlbum(album);
            
        }

        public void RefreshAlbumInfo(Artist artist, IEnumerable<Album> remoteAlbums)
        {
            _logger.Info("Starting album info refresh for: {0}", artist);
            var successCount = 0;
            var failCount = 0;

            var existingAlbums = _albumService.GetAlbumsByArtist(artist.Id);

            var updateList = new List<Album>();
            var newList = new List<Album>();
            var dupeFreeRemoteAlbums = remoteAlbums.DistinctBy(m => new {m.ForeignAlbumId, m.ReleaseDate}).ToList();

            foreach (var album in OrderAlbums(artist, dupeFreeRemoteAlbums))
            {
                
                try
                {
                    var albumToUpdate = GetAlbumToUpdate(artist, album, existingAlbums);

                    Tuple<Album, List<Track>> tuple;
                    var albumInfo = new Album();

                    if (albumToUpdate != null)
                    {
                        tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId, albumToUpdate.CurrentRelease?.Id);
                        albumInfo = tuple.Item1;
                        existingAlbums.Remove(albumToUpdate);
                        updateList.Add(albumToUpdate);
                    }
                    else
                    {
                        tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId, null);
                        albumInfo = tuple.Item1;
                        albumToUpdate = new Album
                        {
                            Monitored = artist.Monitored,
                            ProfileId = artist.ProfileId,
                            Added = DateTime.UtcNow
                        };

                        albumToUpdate.ArtistId = artist.Id;
                        albumToUpdate.CleanTitle = albumInfo.CleanTitle;
                        albumToUpdate.ForeignAlbumId = albumInfo.ForeignAlbumId;
                        albumToUpdate.Title = albumInfo.Title ?? "Unknown";
                        albumToUpdate.AlbumType = albumInfo.AlbumType;
                        albumToUpdate.Releases = albumInfo.Releases;
                        albumToUpdate.CurrentRelease = albumInfo.CurrentRelease;

                        _albumService.AddAlbum(albumToUpdate);

                        newList.Add(albumToUpdate);
                    }
                   
                    
                    albumToUpdate.LastInfoSync = DateTime.UtcNow;
                    albumToUpdate.CleanTitle = albumInfo.CleanTitle;
                    albumToUpdate.Title = albumInfo.Title ?? "Unknown";
                    albumToUpdate.CleanTitle = Parser.Parser.CleanArtistName(albumToUpdate.Title);
                    albumToUpdate.AlbumType = albumInfo.AlbumType;
                    albumToUpdate.SecondaryTypes = albumInfo.SecondaryTypes;
                    albumToUpdate.Genres = albumInfo.Genres;
                    albumToUpdate.Media = albumInfo.Media;
                    albumToUpdate.Label = albumInfo.Label;
                    albumToUpdate.Images = albumInfo.Images;
                    albumToUpdate.ReleaseDate = albumInfo.ReleaseDate;
                    albumToUpdate.Duration = tuple.Item2.Sum(track => track.Duration);
                    albumToUpdate.Releases = albumInfo.Releases;
                    albumToUpdate.CurrentRelease = albumInfo.CurrentRelease;

                    _refreshTrackService.RefreshTrackInfo(albumToUpdate, tuple.Item2);

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating album info for artist {0}. {1}", artist,
                        album);
                    failCount++;
                }
            }

            _albumService.DeleteMany(existingAlbums);
            _albumService.UpdateMany(updateList);
            _albumService.UpdateMany(newList);

            _eventAggregator.PublishEvent(new AlbumInfoRefreshedEvent(artist, newList, updateList));

            if (failCount != 0)
            {
                _logger.Info("Finished album refresh for artist: {0}. Successful: {1} - Failed: {2} ",
                    artist.Name, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished album refresh for artist: {0}.", artist);
            }
        }

        private bool GetMonitoredStatus(Album album, IEnumerable<Artist> artists)
        {
            var artist = artists.SingleOrDefault(c => c.Id == album.ArtistId);
            return album == null || album.Monitored;
        }


        private Album GetAlbumToUpdate(Artist artist, Album album, List<Album> existingAlbums)
        {
            return existingAlbums.FirstOrDefault(
                e => e.ForeignAlbumId == album.ForeignAlbumId /* && e.ReleaseDate == album.ReleaseDate*/);
        }

        private IEnumerable<Album> OrderAlbums(Artist artist, List<Album> albums)
        {
            return albums.OrderBy(e => e.ForeignAlbumId).ThenBy(e => e.ReleaseDate);
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

