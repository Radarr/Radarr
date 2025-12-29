using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Audiobooks.Events
{
    public class AudiobooksDeletedEvent : IEvent
    {
        public List<Audiobook> Audiobooks { get; private set; }
        public bool DeleteFiles { get; private set; }

        public AudiobooksDeletedEvent(List<Audiobook> audiobooks, bool deleteFiles)
        {
            Audiobooks = audiobooks;
            DeleteFiles = deleteFiles;
        }
    }
}
