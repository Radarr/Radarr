using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public class BookAddedHandler : IHandle<BookAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public BookAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(BookAddedEvent message)
        {
            if (message.DoRefresh)
            {
                _commandQueueManager.Push(new RefreshAuthorCommand(message.Book.Author.Value.Id));
            }
        }
    }
}
