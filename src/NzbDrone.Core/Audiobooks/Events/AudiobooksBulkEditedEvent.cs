using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobooksBulkEditedEvent : IEvent
    {
        public IReadOnlyCollection<Audiobook> Audiobooks { get; private set; }

        public AudiobooksBulkEditedEvent(IReadOnlyCollection<Audiobook> audiobooks)
        {
            Audiobooks = audiobooks;
        }
    }
}
