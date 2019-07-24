using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifyException : NzbDroneException
    {
        public SpotifyException(string message) : base(message)
        {
        }

        public SpotifyException(string message, params object[] args) : base(message, args)
        {
        }

        public SpotifyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class SpotifyAuthorizationException : SpotifyException
    {
        public SpotifyAuthorizationException(string message) : base(message)
        {
        }
    }
}
