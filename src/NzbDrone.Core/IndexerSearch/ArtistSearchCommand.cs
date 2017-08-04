using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class ArtistSearchCommand : Command
    {
        public int ArtistId { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
