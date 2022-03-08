using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies.Collections;

namespace NzbDrone.Core.Movies.Events
{
    public class CollectionAddedEvent : IEvent
    {
        public MovieCollection Collection { get; private set; }

        public CollectionAddedEvent(MovieCollection collection)
        {
            Collection = collection;
        }
    }
}
