using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class BulkMoveArtistCommand : Command
    {
        public List<BulkMoveArtist> Artist { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
    }

    public class BulkMoveArtist
    {
        public int ArtistId { get; set; }
        public string SourcePath { get; set; }
    }
}
