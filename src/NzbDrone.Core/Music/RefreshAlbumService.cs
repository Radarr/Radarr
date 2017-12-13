using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using NzbDrone.Core.Organizer;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumService
    {
        void RefreshAlbumInfo(Artist artist, IEnumerable<Album> remoteAlbums);
    }

    public class RefreshAlbumService : IRefreshAlbumService
    {
        private readonly IAlbumService _albumService;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService, IBuildFileNames fileNameBuilder, IRefreshTrackService refreshTrackService, IEventAggregator eventAggregator, Logger logger)
        {
            _albumService = albumService;
            _fileNameBuilder = fileNameBuilder;
            _refreshTrackService = refreshTrackService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshAlbumInfo(Artist artist, IEnumerable<Album> remoteAlbums)
        {
            _logger.Info("Starting album info refresh for: {0}", artist);
            var successCount = 0;
            var failCount = 0;

            var existingAlbums = _albumService.GetAlbumsByArtist(artist.Id);

            var updateList = new List<Album>();
            var newList = new List<Album>();
            var dupeFreeRemoteAlbums = remoteAlbums.DistinctBy(m => new { m.ForeignAlbumId, m.ReleaseDate }).ToList();

            foreach (var album in OrderAlbums(artist, dupeFreeRemoteAlbums))
            {
                try
                {
                    var albumToUpdate = GetAlbumToUpdate(artist, album, existingAlbums);

                    if (albumToUpdate != null)
                    {
                        existingAlbums.Remove(albumToUpdate);
                        updateList.Add(albumToUpdate);
                    }
                    else
                    {
                        albumToUpdate = new Album();
                        albumToUpdate.Monitored = artist.Monitored;
                        albumToUpdate.ProfileId = artist.ProfileId;
                        albumToUpdate.Added = DateTime.UtcNow;

                        newList.Add(albumToUpdate);
                    }


                    albumToUpdate.ForeignAlbumId = album.ForeignAlbumId;
                    albumToUpdate.LastInfoSync = DateTime.UtcNow;
                    albumToUpdate.CleanTitle = album.CleanTitle;
                    albumToUpdate.Title = album.Title ?? "Unknown";
                    albumToUpdate.CleanTitle = Parser.Parser.CleanArtistName(albumToUpdate.Title);
                    albumToUpdate.ArtistId = artist.Id;
                    albumToUpdate.AlbumType = album.AlbumType;
                    albumToUpdate.SecondaryTypes = album.SecondaryTypes;
                    albumToUpdate.Genres = album.Genres;
                    albumToUpdate.Media = album.Media;
                    albumToUpdate.Label = album.Label;
                    albumToUpdate.Images = album.Images;
                    albumToUpdate.ReleaseDate = album.ReleaseDate;
                    albumToUpdate.Duration = album.Tracks.Sum(track => track.Duration);


                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating track info for artist {0}. {1}", artist, album);
                    failCount++;
                }
            }

            var allAlbums = new List<Album>();
            allAlbums.AddRange(newList);
            allAlbums.AddRange(updateList);

            // TODO: See if anything needs to be done here
            //AdjustMultiEpisodeAirTime(artist, allTracks);
            //AdjustDirectToDvdAirDate(artist, allTracks);

            _albumService.DeleteMany(existingAlbums);
            _albumService.UpdateMany(updateList);
            _albumService.InsertMany(newList);
            
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
            return existingAlbums.FirstOrDefault(e => e.ForeignAlbumId == album.ForeignAlbumId/* && e.ReleaseDate == album.ReleaseDate*/);
        }

        private IEnumerable<Album> OrderAlbums(Artist artist, List<Album> albums)
        {
            return albums.OrderBy(e => e.ForeignAlbumId).ThenBy(e => e.ReleaseDate);
        }
    }
}

