using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public class ArtistEditedService : IHandle<ArtistEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public ArtistEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ArtistEditedEvent message)
        {
            // Refresh Artist is we change AlbumType Preferences
            if (message.Artist.MetadataProfileId != message.OldArtist.MetadataProfileId)
            {
                _commandQueueManager.Push(new RefreshArtistCommand(message.Artist.Id, false));
            }
        }
    }
}
