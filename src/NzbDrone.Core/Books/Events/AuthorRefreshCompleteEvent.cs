using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class AuthorRefreshCompleteEvent : IEvent
    {
        public Author Author { get; set; }

        public AuthorRefreshCompleteEvent(Author author)
        {
            Author = author;
        }
    }
}
