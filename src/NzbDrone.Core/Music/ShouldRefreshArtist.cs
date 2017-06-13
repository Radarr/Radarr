using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface ICheckIfArtistShouldBeRefreshed
    {
        bool ShouldRefresh(Artist artist);
    }

    public class CheckIfArtistShouldBeRefreshed : ICheckIfArtistShouldBeRefreshed
    {
        private readonly ITrackService _trackService;
        private readonly Logger _logger;

        public CheckIfArtistShouldBeRefreshed(ITrackService trackService, Logger logger)
        {
            _trackService = trackService;
            _logger = logger;
        }

        public bool ShouldRefresh(Artist artist)
        {
            if (artist.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Artist {0} last updated more than 30 days ago, should refresh.", artist.Name);
                return true;
            }

            if (artist.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
            {
                _logger.Trace("Artist {0} last updated less than 6 hours ago, should not be refreshed.", artist.Name);
                return false;
            }

            //_logger.Trace("Artist {0} ended long ago, should not be refreshed.", artist.Title);
            return false;
        }
    }
}
