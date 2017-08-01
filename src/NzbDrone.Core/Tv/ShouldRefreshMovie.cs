using System;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Tv
{
    public interface ICheckIfMovieShouldBeRefreshed
    {
        bool ShouldRefresh(Movie movie);
    }

    public class ShouldRefreshMovie : ICheckIfMovieShouldBeRefreshed
    {
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public ShouldRefreshMovie(IEpisodeService episodeService, Logger logger)
        {
            _episodeService = episodeService;
            _logger = logger;
        }

        public bool ShouldRefresh(Movie movie)
        {
            return false;
            if (movie.LastInfoSync < DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Movie {0} last updated more than 30 days ago, should refresh.", movie.Title);
                return true;
            }

            if (movie.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
            {
                _logger.Trace("Movie {0} last updated less than 6 hours ago, should not be refreshed.", movie.Title);
                return false;
            }

            if (movie.Status != MovieStatusType.TBA)
            {
                _logger.Trace("Movie {0} is announced or released, should refresh.", movie.Title); //We probably have to change this.
                return true;
            }

            _logger.Trace("Movie {0} ended long ago, should not be refreshed.", movie.Title);
            return false;
        }
    }
}
