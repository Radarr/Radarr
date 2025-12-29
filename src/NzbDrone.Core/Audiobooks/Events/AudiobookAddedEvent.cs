using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobookAddedEvent : IEvent
    {
        public Audiobook Audiobook { get; private set; }

        public AudiobookAddedEvent(Audiobook audiobook)
        {
            Audiobook = audiobook;
        }
    }
}
