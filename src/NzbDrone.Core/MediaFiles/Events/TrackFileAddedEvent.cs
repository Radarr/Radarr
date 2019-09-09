using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFileAddedEvent : IEvent
    {
        public TrackFile TrackFile { get; private set; }

        public TrackFileAddedEvent(TrackFile trackFile)
        {
            TrackFile = trackFile;
        }
    }
}
