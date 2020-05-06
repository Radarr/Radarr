using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public class AlbumAddedHandler : IHandle<AlbumAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public AlbumAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(AlbumAddedEvent message)
        {
            if (message.DoRefresh)
            {
                _commandQueueManager.Push(new RefreshArtistCommand(message.Album.Author.Value.Id));
            }
        }
    }
}
