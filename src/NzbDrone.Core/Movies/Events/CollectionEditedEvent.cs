using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies.Collections;

namespace NzbDrone.Core.Movies.Events
{
    public class CollectionEditedEvent : IEvent
    {
        public MovieCollection Collection { get; private set; }
        public MovieCollection OldCollection { get; private set; }

        public CollectionEditedEvent(MovieCollection collection, MovieCollection oldCollection)
        {
            Collection = collection;
            OldCollection = oldCollection;
        }
    }
}
