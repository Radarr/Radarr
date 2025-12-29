using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class EpisodesBulkEditedEvent : IEvent
    {
        public List<Episode> Episodes { get; private set; }

        public EpisodesBulkEditedEvent(List<Episode> episodes)
        {
            Episodes = episodes;
        }
    }
}
