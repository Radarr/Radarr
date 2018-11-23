using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class RefreshMovieCommand : Command
    {
        public int? MovieId { get; set; }
        public bool IsNewMovie { get; set; }

        public RefreshMovieCommand()
        {
        }

        public RefreshMovieCommand(int? movieId, bool isNewMovie = false)
        {
            MovieId = movieId;
            IsNewMovie = isNewMovie;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !MovieId.HasValue;
    }
}
