using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameMovieFolderCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient => false;

        public RenameMovieFolderCommand(List<int> ids)
        {
        MovieIds = ids;
        }
    }
}
