using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanMovieCommand : Command
    {
        public int? MovieId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanMovieCommand()
        {
        }

        public RescanMovieCommand(int movieId)
        {
            MovieId = movieId;
        }
    }
}
