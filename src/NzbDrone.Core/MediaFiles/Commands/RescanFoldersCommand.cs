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
            AddNewArtists = true;
        }

        public RescanFoldersCommand(List<string> folders, FilterFilesType filter, bool addNewArtists, List<int> artistIds)
        {
            Folders = folders;
            Filter = filter;
            AddNewArtists = addNewArtists;
            ArtistIds = artistIds;
        }

        public List<string> Folders { get; set; }
        public FilterFilesType Filter { get; set; }
        public bool AddNewArtists { get; set; }
        public List<int> ArtistIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
