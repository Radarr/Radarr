using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public class AuthorAddedHandler : IHandle<AuthorAddedEvent>,
                                      IHandle<AuthorsImportedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public AuthorAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(AuthorAddedEvent message)
        {
            if (message.DoRefresh)
            {
                _commandQueueManager.Push(new RefreshAuthorCommand(message.Author.Id, true));
            }
        }

        public void Handle(AuthorsImportedEvent message)
        {
            if (message.DoRefresh)
            {
                _commandQueueManager.Push(new BulkRefreshAuthorCommand(message.AuthorIds, true));
            }
        }
    }
}
