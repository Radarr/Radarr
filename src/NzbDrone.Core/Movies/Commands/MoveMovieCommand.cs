using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class MoveMovieCommand : Command
    {
        public int MovieId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool RequiresDiskAccess => true;
    }
}
