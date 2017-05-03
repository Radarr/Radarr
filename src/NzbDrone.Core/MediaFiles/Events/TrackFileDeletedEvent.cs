using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFileDeletedEvent : IEvent
    {
        public TrackFile TrackFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public TrackFileDeletedEvent(TrackFile trackFile, DeleteMediaFileReason reason)
        {
            TrackFile = trackFile;
            Reason = reason;
        }
    }
}
