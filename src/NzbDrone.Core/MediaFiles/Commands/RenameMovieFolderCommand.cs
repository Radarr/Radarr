using NzbDrone.Core.Messaging.Commands;
using System.Collections.Generic;

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
