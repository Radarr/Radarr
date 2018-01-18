using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface IRefreshTrackService
    {
        void RefreshTrackInfo(Album album, IEnumerable<Track> remoteTracks);
    }

    public class RefreshTrackService : IRefreshTrackService
    {
        private readonly ITrackService _trackService;
        private readonly IAlbumService _albumService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshTrackService(ITrackService trackService, IAlbumService albumService, IEventAggregator eventAggregator, Logger logger)
        {
            _trackService = trackService;
            _albumService = albumService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshTrackInfo(Album album, IEnumerable<Track> remoteTracks)
        {
            _logger.Info("Starting track info refresh for: {0}", album);
            var successCount = 0;
            var failCount = 0;

            album = _albumService.FindById(album.ForeignAlbumId);

            var existingTracks = _trackService.GetTracksByAlbum(album.Id);

            var updateList = new List<Track>();
            var newList = new List<Track>();
            var dupeFreeRemoteTracks = remoteTracks.DistinctBy(m => new { m.ForeignTrackId, m.TrackNumber }).ToList();

            foreach (var track in OrderTracks(album, dupeFreeRemoteTracks))
            {
                try
                {
                    var trackToUpdate = GetTrackToUpdate(album, track, existingTracks);

                    if (trackToUpdate != null)
                    {
                        existingTracks.Remove(trackToUpdate);
                        updateList.Add(trackToUpdate);
                    }
                    else
                    {
                        trackToUpdate = new Track();
                        trackToUpdate.Monitored = album.Monitored;
                        trackToUpdate.Id = track.Id;
                        newList.Add(trackToUpdate);
                    }

                    // TODO: Use object mapper to automatically handle this
                    trackToUpdate.ForeignTrackId = track.ForeignTrackId;
                    trackToUpdate.TrackNumber = track.TrackNumber;
                    trackToUpdate.AbsoluteTrackNumber = track.AbsoluteTrackNumber;
                    trackToUpdate.Title = track.Title ?? "Unknown";
                    trackToUpdate.AlbumId = album.Id;
                    trackToUpdate.ArtistId = album.ArtistId;
                    trackToUpdate.Album = track.Album ?? album;
                    trackToUpdate.Explicit = track.Explicit;
                    trackToUpdate.ArtistId = album.ArtistId;
                    trackToUpdate.Compilation = track.Compilation;
                    trackToUpdate.Duration = track.Duration;
                    trackToUpdate.MediumNumber = track.MediumNumber;


                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating track info for album {0}. {1}", album, track);
                    failCount++;
                }
            }

            var allTracks = new List<Track>();
            allTracks.AddRange(newList);
            allTracks.AddRange(updateList);

            _trackService.DeleteMany(existingTracks);
            _trackService.UpdateMany(updateList);
            _trackService.InsertMany(newList);

            _eventAggregator.PublishEvent(new TrackInfoRefreshedEvent(album, newList, updateList, existingTracks));

            if (failCount != 0)
            {
                _logger.Info("Finished track refresh for album: {0}. Successful: {1} - Failed: {2} ",
                    album.Title, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished track refresh for album: {0}.", album);
            }
        }

        private bool GetMonitoredStatus(Track track, IEnumerable<Album> albums)
        {
            if (track.AbsoluteTrackNumber == 0 /*&& track.AlbumId != 1*/)
            {
                return false;
            }

            var album = albums.SingleOrDefault(c => c.Id == track.AlbumId);
            return album == null || album.Monitored;
        }


        private Track GetTrackToUpdate(Album album, Track track, List<Track> existingTracks)
        {
            var result = existingTracks.FirstOrDefault(e => e.ForeignTrackId == track.ForeignTrackId && e.TrackNumber == track.TrackNumber);
            return result;
        }

        private IEnumerable<Track> OrderTracks(Album album, List<Track> tracks)
        {
            return tracks.OrderBy(e => e.AlbumId).ThenBy(e => e.TrackNumber);
        }
    }
}

