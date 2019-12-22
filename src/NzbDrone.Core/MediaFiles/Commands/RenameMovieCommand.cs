using NzbDrone.Core.Messaging.Commands;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameMovieCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameMovieCommand()
        {
        }
    }
}
