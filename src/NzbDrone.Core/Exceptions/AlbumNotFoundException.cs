using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class AlbumNotFoundException : NzbDroneException
    {
        public string MusicBrainzId { get; set; }

        public AlbumNotFoundException(string musicbrainzId)
            : base(string.Format("Album with id {0} was not found, it may have been removed from metadata server.", musicbrainzId))
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
