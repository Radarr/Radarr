using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistRenamedEvent : IEvent
    {
        public Artist Artist { get; private set; }

        public ArtistRenamedEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
