using System.Linq;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public class ArtistAddedHandler : IHandle<ArtistAddedEvent>,
                                      IHandle<ArtistsImportedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public ArtistAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ArtistAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshArtistCommand(message.Artist.Id, true));
        }

        public void Handle(ArtistsImportedEvent message)
        {
            _commandQueueManager.PushMany(message.ArtistIds.Select(s => new RefreshArtistCommand(s, true)).ToList());
        }
    }
}
