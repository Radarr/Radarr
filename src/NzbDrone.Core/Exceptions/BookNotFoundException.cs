using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class BookNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public BookNotFoundException(string musicbrainzId)
            : base(string.Format("Book with id {0} was not found, it may have been removed from metadata server.", musicbrainzId))
        {
            MusicBrainzId = musicbrainzId;
        }

        public BookNotFoundException(string musicbrainzId, string message, params object[] args)
            : base(message, args)
        {
            MusicBrainzId = musicbrainzId;
        }

        public BookNotFoundException(string musicbrainzId, string message)
            : base(message)
        {
            MusicBrainzId = musicbrainzId;
        }
    }
}
