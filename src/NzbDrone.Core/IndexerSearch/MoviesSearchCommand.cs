using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MoviesSearchCommand : Command
    {
        public int MovieId { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}