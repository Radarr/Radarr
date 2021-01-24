using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanFoldersCommand : Command
    {
        public RescanFoldersCommand()
        {
            // These are the settings used in the scheduled task
            Filter = FilterFilesType.Known;
            AddNewAuthors = true;
        }

        public RescanFoldersCommand(List<string> folders, FilterFilesType filter, bool addNewAuthors, List<int> authorIds)
        {
            Folders = folders;
            Filter = filter;
            AddNewAuthors = addNewAuthors;
            AuthorIds = authorIds;
        }

        public List<string> Folders { get; set; }
        public FilterFilesType Filter { get; set; }
        public bool AddNewAuthors { get; set; }
        public List<int> AuthorIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
