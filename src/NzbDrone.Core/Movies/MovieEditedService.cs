using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class MovieEditedService : IHandle<MovieEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public MovieEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(MovieEditedEvent message)
        {
            if (message.Movie.ImdbId != message.OldMovie.ImdbId)
            {
                _commandQueueManager.Push(new RefreshMovieCommand(message.Movie.Id)); //Probably not needed, as metadata should stay the same.
            }
        }
    }
}
