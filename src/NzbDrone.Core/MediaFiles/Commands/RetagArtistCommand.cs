using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RetagArtistCommand : Command
    {
        public List<int> ArtistIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RetagArtistCommand()
        {
            ArtistIds = new List<int>();
        }
    }
}
