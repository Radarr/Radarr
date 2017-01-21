using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingMoviesSearchCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
