using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class ArtistNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public ArtistNotFoundException(string musicbrainzId)
            : base(string.Format("Artist with id {0} was not found, it may have been removed from the metadata server.", musicbrainzId))
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
