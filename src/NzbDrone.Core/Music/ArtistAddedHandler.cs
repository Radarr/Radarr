using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class ArtistAddedHandler : IHandle<ArtistAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public ArtistAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ArtistAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshArtistCommand(message.Artist.Id));
        }
    }
}
