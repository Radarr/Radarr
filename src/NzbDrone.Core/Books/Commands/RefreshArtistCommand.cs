using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class RefreshArtistCommand : Command
    {
        public int? AuthorId { get; set; }
        public bool IsNewArtist { get; set; }

        public RefreshArtistCommand()
        {
        }

        public RefreshArtistCommand(int? authorId, bool isNewArtist = false)
        {
            AuthorId = authorId;
            IsNewArtist = isNewArtist;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !AuthorId.HasValue;
    }
}
