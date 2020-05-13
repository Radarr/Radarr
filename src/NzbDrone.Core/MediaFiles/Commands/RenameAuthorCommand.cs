using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameAuthorCommand : Command
    {
        public List<int> AuthorIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameAuthorCommand()
        {
            AuthorIds = new List<int>();
        }
    }
}
