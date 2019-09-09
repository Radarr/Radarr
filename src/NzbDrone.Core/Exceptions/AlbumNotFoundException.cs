using NzbDrone.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    public class AlbumNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public AlbumNotFoundException(string musicbrainzId)
            : base(string.Format("Album with MusicBrainz {0} was not found, it may have been removed from MusicBrainz.", musicbrainzId))
        {
            MusicBrainzId = musicbrainzId;
        }

        public AlbumNotFoundException(string musicbrainzId, string message, params object[] args)
            : base(message, args)
        {
            MusicBrainzId = musicbrainzId;
        }

        public AlbumNotFoundException(string musicbrainzId, string message)
            : base(message)
        {
            MusicBrainzId = musicbrainzId;
        }
    }
}
