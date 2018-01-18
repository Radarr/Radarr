using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class RefreshAlbumCommand : Command
    {
        public int? AlbumId { get; set; }

        public RefreshAlbumCommand()
        {
        }

        public RefreshAlbumCommand(int? albumId)
        {
            AlbumId = albumId;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !AlbumId.HasValue;
    }
}
