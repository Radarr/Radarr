using System;
using NLog;

namespace NzbDrone.Core.Movies
{
    public interface ICheckIfMovieShouldBeRefreshed
    {
        bool ShouldRefresh(MovieMetadata movie);
    }

    public class ShouldRefreshMovie : ICheckIfMovieShouldBeRefreshed
    {
        private readonly Logger _logger;

        public ShouldRefreshMovie(Logger logger)
        {
            _logger = logger;
        }

        public bool ShouldRefresh(MovieMetadata movie)
        {
            try
            {
                if (movie == null)
                {
                    _logger.Warn("Movie metadata does not exist, should not be refreshed.");
                    return false;
                }

                if (movie.LastInfoSync < DateTime.UtcNow.AddDays(-180))
                {
                    _logger.Trace("Movie {0} last updated more than 180 days ago, should refresh.", movie.Title);
                    return true;
                }

                if (movie.LastInfoSync >= DateTime.UtcNow.AddHours(-12))
                {
                    _logger.Trace("Movie {0} last updated less than 12 hours ago, should not be refreshed.",
                        movie.Title);
                    return false;
                }

                if (movie.Status is MovieStatusType.Announced or MovieStatusType.InCinemas)
                {
                    _logger.Trace("Movie {0} is announced or in cinemas, should refresh.", movie.Title);
                    return true;
                }

                if (movie.Status == MovieStatusType.Released &&
                    movie.PhysicalReleaseDate() >= DateTime.UtcNow.AddDays(-30))
                {
                    _logger.Trace("Movie {0} is released since less than 30 days, should refresh", movie.Title);
                    return true;
                }

                _logger.Trace("Movie {0} came out long ago, should not be refreshed.", movie.Title);
                return false;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to determine if movie metadata should refresh, will try to refresh.");
                return true;
            }
        }
    }
}
