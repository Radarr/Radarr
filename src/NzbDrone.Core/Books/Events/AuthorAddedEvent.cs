using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class AuthorAddedEvent : IEvent
    {
        public Author Author { get; private set; }
        public bool DoRefresh { get; private set; }

        public AuthorAddedEvent(Author author, bool doRefresh = true)
        {
            Author = author;
            DoRefresh = doRefresh;
        }
    }
}
