using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RetagArtistCommand : Command
    {
        public List<int> AuthorIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RetagArtistCommand()
        {
            AuthorIds = new List<int>();
        }
    }
}
