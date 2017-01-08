using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameMovieCommand : Command
    {
        public int MovieId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RenameMovieCommand()
        {
        }
    }
}
