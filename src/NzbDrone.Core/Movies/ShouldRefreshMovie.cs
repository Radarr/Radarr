using System;
using System.Linq;
using NLog;

namespace NzbDrone.Core.Movies
{
    public interface ICheckIfMovieShouldBeRefreshed
    {
        bool ShouldRefresh(Movie movie);
    }

    public class ShouldRefreshMovie : ICheckIfMovieShouldBeRefreshed
    {
        private readonly Logger _logger;

        public ShouldRefreshMovie(Logger logger)
        {
            _logger = logger;
        }

        public bool ShouldRefresh(Movie movie)
        {
            //return false;
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

            if (movie.Status == MovieStatusType.Announced || movie.Status == MovieStatusType.InCinemas)
            {
                _logger.Trace("Movie {0} is announced or in cinemas, should refresh.", movie.Title); //We probably have to change this.
                return true;
            }

            if (movie.Status == MovieStatusType.Released && movie.PhysicalReleaseDate() >= DateTime.UtcNow.AddDays(-30))
            {
                _logger.Trace("Movie {0} is released since less than 30 days, should refresh", movie.Title);
                return true;
            }

            _logger.Trace("Movie {0} came out long ago, should not be refreshed.", movie.Title);
            return false;
        }
    }
}
