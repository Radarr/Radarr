using NzbDrone.Core.Messaging.Commands;
using System.Collections.Generic;

namespace NzbDrone.Core.IndexerSearch
{
    public class MoviesSearchCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
