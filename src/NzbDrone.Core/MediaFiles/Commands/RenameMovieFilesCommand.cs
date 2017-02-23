using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameMovieFilesCommand : Command
    {
        public int MovieId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient => true;

        public RenameMovieFilesCommand()
        {
        }

        public RenameMovieFilesCommand(int movieId, List<int> files)
        {
            MovieId = movieId;
            Files = files;
        }
    }
}
