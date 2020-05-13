using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AuthorScanSkippedEvent : IEvent
    {
        public Author Author { get; private set; }
        public AuthorScanSkippedReason Reason { get; private set; }

        public AuthorScanSkippedEvent(Author author, AuthorScanSkippedReason reason)
        {
            Author = author;
            Reason = reason;
        }
    }

    public enum AuthorScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty
    }
}
