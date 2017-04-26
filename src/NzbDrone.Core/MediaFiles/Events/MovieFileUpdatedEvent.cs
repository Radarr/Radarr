using NzbDrone.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileUpdatedEvent : IEvent
    {
        public MovieFile MovieFile { get; private set; }

        public MovieFileUpdatedEvent(MovieFile movieFile)
        {
            MovieFile = movieFile;
        }
    }
}
