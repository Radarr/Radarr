using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumAddedEvent : IEvent
    {
        public Album Album { get; private set; }

        public AlbumAddedEvent(Album album)
        {
            Album = album;
        }
    }
}
