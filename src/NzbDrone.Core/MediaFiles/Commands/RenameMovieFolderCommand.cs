using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameMovieFolderCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient => false;

        public RenameMovieFolderCommand()
        {
        }

        public RenameMovieFolderCommand(List<int> ids)
        {
            MovieIds = ids;
        }
    }
}
