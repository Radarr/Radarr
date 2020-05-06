using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class AlbumSearchCommand : Command
    {
        public List<int> BookIds { get; set; }

        public override bool SendUpdatesToClient => true;

        public AlbumSearchCommand()
        {
        }

        public AlbumSearchCommand(List<int> bookIds)
        {
            BookIds = bookIds;
        }
    }
}
