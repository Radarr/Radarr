using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistScanSkippedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public ArtistScanSkippedReason Reason { get; private set; }

        public ArtistScanSkippedEvent(Artist artist, ArtistScanSkippedReason reason)
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
