using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class AuthorMovedEvent : IEvent
    {
        public Author Author { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public AuthorMovedEvent(Author author, string sourcePath, string destinationPath)
        {
            Author = author;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
