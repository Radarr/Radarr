using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumDeletedEvent : IEvent
    {
        public Album Album { get; private set; }
        public bool DeleteFiles { get; private set; }

        public AlbumDeletedEvent(Album album, bool deleteFiles)
        {
            Album = album;
            DeleteFiles = deleteFiles;
        }
    }
}
