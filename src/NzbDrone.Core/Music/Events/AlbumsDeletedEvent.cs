using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumsDeletedEvent : IEvent
    {
        public List<Album> Albums { get; private set; }
        public bool DeleteFiles { get; private set; }

        public AlbumsDeletedEvent(List<Album> albums, bool deleteFiles)
        {
            Albums = albums;
            DeleteFiles = deleteFiles;
        }
    }
}
