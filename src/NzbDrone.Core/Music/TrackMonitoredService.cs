using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface ITrackMonitoredService
    {
        void SetTrackMonitoredStatus(Artist album, MonitoringOptions monitoringOptions);
    }

    public class TrackMonitoredService : ITrackMonitoredService
    {
        private readonly IArtistService _albumService;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;

        public TrackMonitoredService(IArtistService albumService, ITrackService trackService, Logger logger)
        {
            _albumService = albumService;
            _trackService = trackService;
            _logger = logger;
        }

        public void SetTrackMonitoredStatus(Artist album, MonitoringOptions monitoringOptions)
        {
            if (monitoringOptions != null)
            {
                _logger.Debug("[{0}] Setting track monitored status.", album.Name);

                var tracks = _trackService.GetTracksByArtist(album.Id);

                if (monitoringOptions.IgnoreAlbumsWithFiles)
                {
                    _logger.Debug("Ignoring Tracks with Files");
                    ToggleTracksMonitoredState(tracks.Where(e => e.HasFile), false);
                }

                else
                {
                    _logger.Debug("Monitoring Tracks with Files");
                    ToggleTracksMonitoredState(tracks.Where(e => e.HasFile), true);
                }

                if (monitoringOptions.IgnoreAlbumsWithoutFiles)
                {
                    _logger.Debug("Ignoring Tracks without Files");
                    ToggleTracksMonitoredState(tracks.Where(e => !e.HasFile), false);
                }

                else
                {
                    _logger.Debug("Monitoring Episodes without Files");
                    ToggleTracksMonitoredState(tracks.Where(e => !e.HasFile), true);
                }

                _trackService.UpdateTracks(tracks);
            }

            _albumService.UpdateArtist(album);
        }

        private void ToggleTracksMonitoredState(IEnumerable<Track> tracks, bool monitored)
        {
            foreach (var track in tracks)
            {
                track.Monitored = monitored;
            }
        }
    }
}
