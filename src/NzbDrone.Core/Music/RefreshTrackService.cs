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
        void RefreshTrackInfo(Album rg);
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

        public void RefreshTrackInfo(Album album)
        {
            _logger.Info("Starting track info refresh for: {0}", album);
            var successCount = 0;
            var failCount = 0;

            foreach (var release in album.AlbumReleases.Value)
            {

                var existingTracks = _trackService.GetTracksByForeignReleaseId(release.ForeignReleaseId);

                var updateList = new List<Track>();
                var newList = new List<Track>();
                var dupeFreeRemoteTracks = release.Tracks.Value.DistinctBy(m => new { m.ForeignTrackId, m.TrackNumber }).ToList();

                foreach (var track in OrderTracks(dupeFreeRemoteTracks))
                {
                    try
                    {
                        var trackToUpdate = GetTrackToUpdate(track, existingTracks);

                        if (trackToUpdate != null)
                        {
                            existingTracks.Remove(trackToUpdate);
                            updateList.Add(trackToUpdate);
                        }
                        else
                        {
                            trackToUpdate = new Track();
                            trackToUpdate.Id = track.Id;
                            newList.Add(trackToUpdate);
                        }

                        // TODO: Use object mapper to automatically handle this
                        trackToUpdate.ForeignTrackId = track.ForeignTrackId;
                        trackToUpdate.ForeignRecordingId = track.ForeignRecordingId;
                        trackToUpdate.AlbumReleaseId = release.Id;
                        trackToUpdate.ArtistMetadataId = track.ArtistMetadata.Value.Id;
                        trackToUpdate.TrackNumber = track.TrackNumber;
                        trackToUpdate.AbsoluteTrackNumber = track.AbsoluteTrackNumber;
                        trackToUpdate.Title = track.Title ?? "Unknown";
                        trackToUpdate.Explicit = track.Explicit;
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

                _trackService.DeleteMany(existingTracks);
                _trackService.UpdateMany(updateList);
                _trackService.InsertMany(newList);
            }

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

        private Track GetTrackToUpdate(Track track, List<Track> existingTracks)
        {
            var result = existingTracks.FirstOrDefault(e => e.ForeignTrackId == track.ForeignTrackId && e.TrackNumber == track.TrackNumber);
            return result;
        }

        private IEnumerable<Track> OrderTracks(List<Track> tracks)
        {
            return tracks.OrderBy(e => e.AlbumReleaseId).ThenBy(e => e.TrackNumber);
        }
    }
}

