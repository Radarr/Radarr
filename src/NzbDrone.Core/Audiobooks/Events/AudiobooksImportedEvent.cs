using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobooksImportedEvent : IEvent
    {
        public List<Audiobook> Audiobooks { get; private set; }

        public AudiobooksImportedEvent(List<Audiobook> audiobooks)
        {
            Audiobooks = audiobooks;
        }
    }
}
