using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumInfoRefreshedEvent : IEvent
    {
        public Artist Artist { get; set; }
        public ReadOnlyCollection<Album> Added { get; private set; }
        public ReadOnlyCollection<Album> Updated { get; private set; }

        public AlbumInfoRefreshedEvent(Artist artist, IList<Album> added, IList<Album> updated)
        {
            Artist = artist;
            Added = new ReadOnlyCollection<Album>(added);
            Updated = new ReadOnlyCollection<Album>(updated);
        }
    }
}
