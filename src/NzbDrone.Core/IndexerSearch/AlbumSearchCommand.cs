using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    class AlbumSearchCommand : Command
    {
        public int AlbumId { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
