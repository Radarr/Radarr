using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistScanSkippedEvent : IEvent
    {
        public Author Artist { get; private set; }
        public ArtistScanSkippedReason Reason { get; private set; }

        public ArtistScanSkippedEvent(Author artist, ArtistScanSkippedReason reason)
        {
            Artist = artist;
            Reason = reason;
        }
    }

    public enum ArtistScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty
    }
}
