using System;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Music
{
    public interface ICheckIfArtistShouldBeRefreshed
    {
        bool ShouldRefresh(Author artist);
    }

    public class ShouldRefreshArtist : ICheckIfArtistShouldBeRefreshed
    {
        private readonly IAlbumService _albumService;
        private readonly Logger _logger;

        public ShouldRefreshArtist(IAlbumService albumService, Logger logger)
        {
            _albumService = albumService;
            _logger = logger;
        }

        public bool ShouldRefresh(Author artist)
        {
            if (artist.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Artist {0} last updated more than 30 days ago, should refresh.", artist.Name);
                return true;
            }

            if (artist.LastInfoSync >= DateTime.UtcNow.AddHours(-12))
            {
                _logger.Trace("Artist {0} last updated less than 12 hours ago, should not be refreshed.", artist.Name);
                return false;
            }

            if (artist.Metadata.Value.Status == ArtistStatusType.Continuing && artist.LastInfoSync < DateTime.UtcNow.AddDays(-2))
            {
                _logger.Trace("Artist {0} is continuing and has not been refreshed in 2 days, should refresh.", artist.Name);
                return true;
            }

            var lastAlbum = _albumService.GetAlbumsByArtist(artist.Id).OrderByDescending(e => e.ReleaseDate).FirstOrDefault();

            if (lastAlbum != null && lastAlbum.ReleaseDate > DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Last album in {0} aired less than 30 days ago, should refresh.", artist.Name);
                return true;
            }

            _logger.Trace("Artist {0} ended long ago, should not be refreshed.", artist.Name);
            return false;
        }
    }
}
