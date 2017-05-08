using NzbDrone.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    public class ArtistNotFoundException : NzbDroneException
    {
        public string SpotifyId { get; set; }

        public ArtistNotFoundException(string spotifyId)
            : base(string.Format("Artist with SpotifyId {0} was not found, it may have been removed from Spotify.", spotifyId))
        {
            SpotifyId = spotifyId;
        }

        public ArtistNotFoundException(string spotifyId, string message, params object[] args)
            : base(message, args)
        {
            SpotifyId = spotifyId;
        }

        public ArtistNotFoundException(string spotifyId, string message)
            : base(message)
        {
            SpotifyId = spotifyId;
        }
    }
}
