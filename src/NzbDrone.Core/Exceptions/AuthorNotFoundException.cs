using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class AuthorNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public AuthorNotFoundException(string musicbrainzId)
            : base(string.Format("Author with id {0} was not found, it may have been removed from the metadata server.", musicbrainzId))
        {
            MusicBrainzId = musicbrainzId;
        }

        public AuthorNotFoundException(string musicbrainzId, string message, params object[] args)
            : base(message, args)
        {
            MusicBrainzId = musicbrainzId;
        }

        public AuthorNotFoundException(string musicbrainzId, string message)
            : base(message)
        {
            MusicBrainzId = musicbrainzId;
        }
    }
}
