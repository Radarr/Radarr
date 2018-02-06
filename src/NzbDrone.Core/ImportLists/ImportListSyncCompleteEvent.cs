using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncCompleteEvent : IEvent
    {
        public List<Album> ProcessedDecisions { get; private set; }

        public ImportListSyncCompleteEvent(List<Album> processedDecisions)
        {
            ProcessedDecisions = processedDecisions;
        }
    }
}
