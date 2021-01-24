using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public class AuthorEditedService : IHandle<AuthorEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public AuthorEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(AuthorEditedEvent message)
        {
            // Refresh Author is we change BookType Preferences
            if (message.Author.MetadataProfileId != message.OldAuthor.MetadataProfileId)
            {
                _commandQueueManager.Push(new RefreshAuthorCommand(message.Author.Id, false));
            }
        }
    }
}
