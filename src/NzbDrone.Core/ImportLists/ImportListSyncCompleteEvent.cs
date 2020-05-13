using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncCompleteEvent : IEvent
    {
        public List<Book> ProcessedDecisions { get; private set; }

        public ImportListSyncCompleteEvent(List<Book> processedDecisions)
        {
            ProcessedDecisions = processedDecisions;
        }
    }
}
