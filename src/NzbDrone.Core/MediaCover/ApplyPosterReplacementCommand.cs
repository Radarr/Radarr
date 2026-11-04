using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaCover
{
    public class ApplyPosterReplacementCommand : Command
    {
        public override bool SendUpdatesToClient => true;

        public override string CompletionMessage => "Completed";
    }
}
