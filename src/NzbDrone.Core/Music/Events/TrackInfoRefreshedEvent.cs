using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class TrackInfoRefreshedEvent : IEvent
    {
        public Artist Artist { get; set; }
        public ReadOnlyCollection<Track> Added { get; private set; }
        public ReadOnlyCollection<Track> Updated { get; private set; }

        public TrackInfoRefreshedEvent(Artist artist, IList<Track> added, IList<Track> updated)
        {
            Artist = artist;
            Added = new ReadOnlyCollection<Track>(added);
            Updated = new ReadOnlyCollection<Track>(updated);
        }
    }
}
