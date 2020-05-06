using System.Collections.Generic;
using System.Collections.ObjectModel;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumInfoRefreshedEvent : IEvent
    {
        public Author Artist { get; set; }
        public ReadOnlyCollection<Book> Added { get; private set; }
        public ReadOnlyCollection<Book> Updated { get; private set; }

        public AlbumInfoRefreshedEvent(Author artist, IList<Book> added, IList<Book> updated)
        {
            Artist = artist;
            Added = new ReadOnlyCollection<Book>(added);
            Updated = new ReadOnlyCollection<Book>(updated);
        }
    }
}
