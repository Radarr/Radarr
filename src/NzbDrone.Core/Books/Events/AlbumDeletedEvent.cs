using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumDeletedEvent : IEvent
    {
        public Book Album { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public AlbumDeletedEvent(Book album, bool deleteFiles, bool addImportListExclusion)
        {
            Album = album;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
