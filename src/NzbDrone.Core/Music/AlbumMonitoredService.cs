using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

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

                var monitoredAlbums = artist.Albums;

                if (monitoredAlbums != null)
                {
                    ToggleAlbumsMonitoredState(
                        albums.Where(s => monitoredAlbums.Any(t => t.ForeignAlbumId == s.ForeignAlbumId)), true);
                    ToggleAlbumsMonitoredState(
                        albums.Where(s => monitoredAlbums.Any(t => t.ForeignAlbumId != s.ForeignAlbumId)), false);
                }
                else
                {
                    ToggleAlbumsMonitoredState(albums, monitoringOptions.Monitored);
                }

                //TODO Add Other Options for Future/Exisitng/Missing Once we have a good way to check for Album Related Files.

                _albumService.UpdateAlbums(albums);
            }

            _artistService.UpdateArtist(artist);
        }

        private void ToggleAlbumsMonitoredState(IEnumerable<Album> albums, bool monitored)
        {
            foreach (var album in albums)
            {
                album.Monitored = monitored;
                var tracks = _trackService.GetTracksByAlbum(album.Id);
                foreach (var track in tracks)
                {
                    track.Monitored = monitored;
                }
                _trackService.UpdateTracks(tracks);
            }
        }
    }
}
