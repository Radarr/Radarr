using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameArtistCommand : Command
    {
        public List<int> ArtistIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameArtistCommand()
        {
            ArtistIds = new List<int>();
        }
    }
}
