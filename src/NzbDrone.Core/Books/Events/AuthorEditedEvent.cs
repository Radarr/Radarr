using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class AuthorEditedEvent : IEvent
    {
        public Author Author { get; private set; }
        public Author OldAuthor { get; private set; }

        public AuthorEditedEvent(Author author, Author oldAuthor)
        {
            Author = author;
            OldAuthor = oldAuthor;
        }
    }
}
