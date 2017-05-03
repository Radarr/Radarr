using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistAddedEvent : IEvent
    {
        public Artist Artist { get; private set; }

        public ArtistAddedEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
