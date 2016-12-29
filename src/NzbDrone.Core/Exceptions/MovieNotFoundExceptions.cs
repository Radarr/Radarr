using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class MovieNotFoundException : NzbDroneException
    {
        public string ImdbId { get; set; }

        public MovieNotFoundException(string imdbid)
            : base(string.Format("Movie with imdbid {0} was not found, it may have been removed from IMDb.", imdbid))
        {
            ImdbId = imdbid;
        }

        public MovieNotFoundException(string imdbid, string message, params object[] args)
            : base(message, args)
        {
            ImdbId = imdbid;
        }

        public MovieNotFoundException(string imdbid, string message)
            : base(message)
        {
            ImdbId = imdbid;
        }
    }
}
