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
        public Album Album { get; set; }
        public ReadOnlyCollection<Track> Added { get; private set; }
        public ReadOnlyCollection<Track> Updated { get; private set; }

        public TrackInfoRefreshedEvent(Album album, IList<Track> added, IList<Track> updated)
        {
            Album = album;
            Added = new ReadOnlyCollection<Track>(added);
            Updated = new ReadOnlyCollection<Track>(updated);
        }
    }
}
