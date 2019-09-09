using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RetagFilesCommand : Command
    {
        public int ArtistId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RetagFilesCommand()
        {
        }

        public RetagFilesCommand(int artistId, List<int> files)
        {
            ArtistId = artistId;
            Files = files;
        }
    }
}
