using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class BookSearchCommand : Command
    {
        public List<int> BookIds { get; set; }

        public override bool SendUpdatesToClient => true;

        public BookSearchCommand()
        {
        }

        public BookSearchCommand(List<int> bookIds)
        {
            BookIds = bookIds;
        }
    }
}
