using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class EditionDeletedEvent : IEvent
    {
        public Edition Edition { get; private set; }

        public EditionDeletedEvent(Edition edition)
        {
            Edition = edition;
        }
    }
}
