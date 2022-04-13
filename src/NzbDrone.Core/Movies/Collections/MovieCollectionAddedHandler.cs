using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class MovieCollectionAddedHandler : IHandle<CollectionAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public MovieCollectionAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(CollectionAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshCollectionsCommand(new List<int> { message.Collection.Id }));
        }
    }
}
