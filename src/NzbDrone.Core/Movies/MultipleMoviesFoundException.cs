using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Movies
{
    public class MultipleMoviesFoundException : NzbDroneException
    {
        public MultipleMoviesFoundException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
