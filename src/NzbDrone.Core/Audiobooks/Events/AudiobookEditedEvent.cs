using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobookEditedEvent : IEvent
    {
        public Audiobook Audiobook { get; private set; }
        public Audiobook OldAudiobook { get; private set; }

        public AudiobookEditedEvent(Audiobook audiobook, Audiobook oldAudiobook)
        {
            Audiobook = audiobook;
            OldAudiobook = oldAudiobook;
        }
    }
}
