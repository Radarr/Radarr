using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobookDeletedEvent : IEvent
    {
        public Audiobook Audiobook { get; private set; }
        public bool DeleteFiles { get; private set; }

        public AudiobookDeletedEvent(Audiobook audiobook, bool deleteFiles)
        {
            Audiobook = audiobook;
            DeleteFiles = deleteFiles;
        }
    }
}
