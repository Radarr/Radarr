using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies.Collections;

namespace NzbDrone.Core.Movies.Events
{
    public class CollectionDeletedEvent : IEvent
    {
        public MovieCollection Collection { get; private set; }

        public CollectionDeletedEvent(MovieCollection collection)
        {
            Collection = collection;
        }
    }
}
