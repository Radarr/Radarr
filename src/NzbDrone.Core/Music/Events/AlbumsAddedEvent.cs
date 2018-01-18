using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumsAddedEvent : IEvent
    {
    public List<int> AlbumIds { get; private set; }

    public AlbumsAddedEvent(List<int> albumIds)
    {
        AlbumIds = albumIds;
    }
    }
}
