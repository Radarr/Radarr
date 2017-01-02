using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScanSkippedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public MovieScanSkippedReason Reason { get; set; }

        public MovieScanSkippedEvent(Movie movie, MovieScanSkippedReason reason)
        {
            Movie = movie;
            Reason = reason;
        }
    }

    public enum MovieScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty,
        MovieFolderDoesNotExist
    }
}
