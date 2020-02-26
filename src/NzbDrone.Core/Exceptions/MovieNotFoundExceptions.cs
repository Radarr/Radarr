using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class MovieNotFoundException : NzbDroneException
    {
        public int TmdbMovieId { get; set; }

        public MovieNotFoundException(int tmdbMovieId)
            : base(string.Format("Movie with tmdbId {0} was not found, it may have been removed from TMDb.", tmdbMovieId))
        {
            TmdbMovieId = tmdbMovieId;
        }

        public MovieNotFoundException(string imdbId)
            : base(string.Format("Movie with IMDBId {0} was not found, it may have been removed from TMDb.", imdbId))
        {
            TmdbMovieId = 0;
        }

        public MovieNotFoundException(int tmdbMovieId, string message, params object[] args)
            : base(message, args)
        {
            TmdbMovieId = tmdbMovieId;
        }

        public MovieNotFoundException(int tmdbMovieId, string message)
            : base(message)
        {
            TmdbMovieId = tmdbMovieId;
        }
    }
}
