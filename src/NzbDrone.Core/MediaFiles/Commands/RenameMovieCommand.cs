using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
