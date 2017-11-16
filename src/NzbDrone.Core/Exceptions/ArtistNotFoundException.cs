using NzbDrone.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    public class ArtistNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public ArtistNotFoundException(string musicbrainzId)
            : base(string.Format("Artist with MusicBrainz {0} was not found, it may have been removed from MusicBrainz.", musicbrainzId))
        {
            MusicBrainzId = musicbrainzId;
        }

        public ArtistNotFoundException(string musicbrainzId, string message, params object[] args)
            : base(message, args)
        {
            MusicBrainzId = musicbrainzId;
        }

        public ArtistNotFoundException(string musicbrainzId, string message)
            : base(message)
        {
            MusicBrainzId = musicbrainzId;
        }
    }
}
