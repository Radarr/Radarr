using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class RefreshAlbumCommand : Command
    {
        public int? BookId { get; set; }

        public RefreshAlbumCommand()
        {
        }

        public RefreshAlbumCommand(int? bookId)
        {
            BookId = bookId;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !BookId.HasValue;
    }
}
