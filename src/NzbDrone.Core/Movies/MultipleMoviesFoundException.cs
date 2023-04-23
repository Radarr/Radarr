using System.Collections.Generic;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Movies
{
    public class MultipleMoviesFoundException : NzbDroneException
    {
        public List<Movie> Movies { get; set; }

        public MultipleMoviesFoundException(List<Movie> movies, string message, params object[] args)
            : base(message, args)
        {
            Movies = movies;
        }
    }
}
