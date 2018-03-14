using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource.PreDB
{
    public class PreDBSyncCompleteEvent : IEvent
    {
        public List<Movie> NewlyReleased { get; private set; }

        public PreDBSyncCompleteEvent(List<Movie> newlyReleased)
        {
            NewlyReleased = newlyReleased;
        }
    }
}
