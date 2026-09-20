using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.AutoTagging
{
    public class AutoTagMoviesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public override bool IsLongRunning => true;
    }
}
