using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class ReleaseDeletedEvent : IEvent
    {
        public AlbumRelease Release { get; private set; }

        public ReleaseDeletedEvent(AlbumRelease release)
        {
            Release = release;
        }
    }
}
