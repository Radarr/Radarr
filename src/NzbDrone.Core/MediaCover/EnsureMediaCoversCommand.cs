using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaCover
{
    public class EnsureMediaCoversCommand : Command
    {
        public int MovieId { get; set; }

        public EnsureMediaCoversCommand()
        {
        }

        public EnsureMediaCoversCommand(int movieId)
        {
            MovieId = movieId;
        }
    }
}
