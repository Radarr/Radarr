using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class RefreshArtistCommand : Command
    {
        public int? ArtistId { get; set; }
        public bool IsNewArtist { get; set; }

        public RefreshArtistCommand()
        {
        }

        public RefreshArtistCommand(int? artistId, bool isNewArtist = false)
        {
            ArtistId = artistId;
            IsNewArtist = isNewArtist;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !ArtistId.HasValue;
    }
}
