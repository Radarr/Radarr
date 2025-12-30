using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class TVShowsBulkEditedEvent : IEvent
    {
        public List<TVShow> TVShows { get; private set; }

        public TVShowsBulkEditedEvent(List<TVShow> tvShows)
        {
            TVShows = tvShows;
        }
    }
}
