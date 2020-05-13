using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Books.Commands
{
    public class MoveAuthorCommand : Command
    {
        public int AuthorId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
