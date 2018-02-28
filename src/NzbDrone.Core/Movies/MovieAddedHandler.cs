using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class MovieAddedHandler : IHandle<MovieAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public MovieAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(MovieAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshMovieCommand(message.Movie.Id));
        }
    }
}
