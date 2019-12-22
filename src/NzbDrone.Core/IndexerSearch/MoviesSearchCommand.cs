using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MoviesSearchCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
