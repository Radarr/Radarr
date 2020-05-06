using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class MoveArtistCommand : Command
    {
        public int AuthorId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
