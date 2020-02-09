using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanFoldersCommand : Command
    {
        public RescanFoldersCommand()
        {
        }

        public RescanFoldersCommand(List<string> folders, FilterFilesType filter, List<int> artistIds)
        {
            Folders = folders;
            Filter = filter;
            ArtistIds = artistIds;
        }

        public List<string> Folders { get; set; }
        public FilterFilesType Filter { get; set; }
        public List<int> ArtistIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
