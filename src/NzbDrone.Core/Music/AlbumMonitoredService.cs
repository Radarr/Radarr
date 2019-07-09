using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public interface IAlbumMonitoredService
    {
        void SetAlbumMonitoredStatus(Artist artist, MonitoringOptions monitoringOptions);
    }

    public class AlbumMonitoredService : IAlbumMonitoredService
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;

        public AlbumMonitoredService(IArtistService artistService, IAlbumService albumService, ITrackService trackService, Logger logger)
        {
            _artistService = artistService;
            _albumService = albumService;
            _trackService = trackService;
            _logger = logger;
        }

        public void SetAlbumMonitoredStatus(Artist artist, MonitoringOptions monitoringOptions)
        {
            if (monitoringOptions != null)
            {
                _logger.Debug("[{0}] Setting album monitored status.", artist.Name);

                var albums = _albumService.GetAlbumsByArtist(artist.Id);

                var albumsWithFiles = _albumService.GetArtistAlbumsWithFiles(artist);

                var albumsWithoutFiles = albums.Where(c => !albumsWithFiles.Select(e => e.Id).Contains(c.Id) && c.ReleaseDate <= DateTime.UtcNow).ToList();

                var monitoredAlbums = monitoringOptions.AlbumsToMonitor;

                // If specific albums are passed use those instead of the monitoring options.
                if (monitoredAlbums.Any())
                {
                    ToggleAlbumsMonitoredState(
                        albums.Where(s => monitoredAlbums.Any(t => t == s.ForeignAlbumId)), true);
                    ToggleAlbumsMonitoredState(
                        albums.Where(s => monitoredAlbums.Any(t => t != s.ForeignAlbumId)), false);
                }
                else
                {
                    switch (monitoringOptions.Monitor)
                    {
                        case MonitorTypes.All:
                            ToggleAlbumsMonitoredState(albums, true);
                            break;
                        case MonitorTypes.Future:
                            _logger.Debug("Unmonitoring Albums with Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithFiles.Select(c => c.Id).Contains(e.Id)), false);
                            _logger.Debug("Unmonitoring Albums without Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithoutFiles.Select(c => c.Id).Contains(e.Id)), false);
                            break;
                        case MonitorTypes.None:
                            ToggleAlbumsMonitoredState(albums, false);
                            break;
                        case MonitorTypes.Missing:
                            _logger.Debug("Unmonitoring Albums with Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithFiles.Select(c => c.Id).Contains(e.Id)), false);
                            _logger.Debug("Monitoring Albums without Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithoutFiles.Select(c => c.Id).Contains(e.Id)), true);
                            break;
                        case MonitorTypes.Existing:
                            _logger.Debug("Monitoring Albums with Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithFiles.Select(c => c.Id).Contains(e.Id)), true);
                            _logger.Debug("Unmonitoring Albums without Files");
                            ToggleAlbumsMonitoredState(albums.Where(e => albumsWithoutFiles.Select(c => c.Id).Contains(e.Id)), false);
                            break;
                        case MonitorTypes.Latest:
                            ToggleAlbumsMonitoredState(albums, false);
                            ToggleAlbumsMonitoredState(albums.OrderByDescending(e=>e.ReleaseDate).Take(1),true);
                            break;
                        case MonitorTypes.First:
                            ToggleAlbumsMonitoredState(albums, false);
                            ToggleAlbumsMonitoredState(albums.OrderBy(e => e.ReleaseDate).Take(1), true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _albumService.UpdateMany(albums);
            }

            _artistService.UpdateArtist(artist);
        }

        private void ToggleAlbumsMonitoredState(IEnumerable<Album> albums, bool monitored)
        {
            foreach (var album in albums)
            {
                album.Monitored = monitored;
            }
        }
    }
}
