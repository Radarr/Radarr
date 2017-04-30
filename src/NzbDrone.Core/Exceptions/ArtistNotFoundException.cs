using NzbDrone.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    public class ArtistNotFoundException : NzbDroneException
    {
        public int ItunesId { get; set; }

        public ArtistNotFoundException(int itunesId)
            : base(string.Format("Series with iTunesId {0} was not found, it may have been removed from iTunes.", itunesId))
        {
            ItunesId = itunesId;
        }

        public ArtistNotFoundException(int itunesId, string message, params object[] args)
            : base(message, args)
        {
            ItunesId = itunesId;
        }

        public ArtistNotFoundException(int itunesId, string message)
            : base(message)
        {
            ItunesId = itunesId;
        }
    }
}
