using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class UpdateMovieFileQualityCommand : Command
    {
        public IEnumerable<int> MovieFileIds { get; set; }
        
        public override bool SendUpdatesToClient => true;

        public UpdateMovieFileQualityCommand(IEnumerable<int> movieFileIds)
        {
            MovieFileIds = movieFileIds;
        }
    }
}
