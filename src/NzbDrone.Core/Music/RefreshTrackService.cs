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
        void RefreshTrackInfo(Artist artist, IEnumerable<Track> remoteTracks);
    }

    public class RefreshTrackService : IRefreshTrackService
    {
        private readonly ITrackService _trackService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshTrackService(ITrackService trackService, IEventAggregator eventAggregator, Logger logger)
        {
            _trackService = trackService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshTrackInfo(Artist artist, IEnumerable<Track> remoteTracks)
        {
            _logger.Info("Starting track info refresh for: {0}", artist);
            var successCount = 0;
            var failCount = 0;

            var existingTracks = _trackService.GetTracksByArtist(artist.Id);
            var albums = artist.Albums;

            var updateList = new List<Track>();
            var newList = new List<Track>();
            var dupeFreeRemoteTracks = remoteTracks.DistinctBy(m => new { m.AlbumId, m.TrackNumber }).ToList();

            foreach (var track in OrderTracks(artist, dupeFreeRemoteTracks))
            {
                try
                {
                    var trackToUpdate = GetTrackToUpdate(artist, track, existingTracks);

                    if (trackToUpdate != null)
                    {
                        existingTracks.Remove(trackToUpdate);
                        updateList.Add(trackToUpdate);
                    }
                    else
                    {
                        trackToUpdate = new Track();
                        trackToUpdate.Monitored = GetMonitoredStatus(track, albums);
                        newList.Add(trackToUpdate);
                    }

                    trackToUpdate.ForeignTrackId = track.ForeignTrackId;
                    trackToUpdate.TrackNumber = track.TrackNumber;
                    trackToUpdate.Title = track.Title ?? "Unknown";
                    trackToUpdate.AlbumId = track.AlbumId;
                    trackToUpdate.Album = track.Album;
                    trackToUpdate.Explicit = track.Explicit;
                    trackToUpdate.ArtistId = artist.Id;
                    trackToUpdate.Compilation = track.Compilation;

                    // TODO: Implement rest of [RefreshTrackService] fields



                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating track info for artist {0}. {1}", artist, track);
                    failCount++;
                }
            }

            var allTracks = new List<Track>();
            allTracks.AddRange(newList);
            allTracks.AddRange(updateList);

            // TODO: See if anything needs to be done here
            //AdjustMultiEpisodeAirTime(artist, allTracks);
            //AdjustDirectToDvdAirDate(artist, allTracks);

            _trackService.DeleteMany(existingTracks);
            _trackService.UpdateMany(updateList);
            _trackService.InsertMany(newList);

            _eventAggregator.PublishEvent(new TrackInfoRefreshedEvent(artist, newList, updateList));

            if (failCount != 0)
            {
                _logger.Info("Finished track refresh for artist: {0}. Successful: {1} - Failed: {2} ",
                    artist.Name, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished track refresh for artist: {0}.", artist);
            }
        }

        private bool GetMonitoredStatus(Track track, IEnumerable<Album> albums)
        {
            if (track.TrackNumber == 0 /*&& track.AlbumId != 1*/)
            {
                return false;
            }

            var album = albums.SingleOrDefault(c => c.Id == track.AlbumId);
            return album == null || album.Monitored;
        }


        private Track GetTrackToUpdate(Artist artist, Track track, List<Track> existingTracks)
        {
            return existingTracks.FirstOrDefault(e => e.AlbumId == track.AlbumId && e.TrackNumber == track.TrackNumber);
        }

        private IEnumerable<Track> OrderTracks(Artist artist, List<Track> tracks)
        {
            return tracks.OrderBy(e => e.AlbumId).ThenBy(e => e.TrackNumber);
        }
    }
}

