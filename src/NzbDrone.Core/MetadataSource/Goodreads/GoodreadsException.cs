using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    public class GoodreadsException : NzbDroneClientException
    {
        public GoodreadsException(string message)
            : base(HttpStatusCode.ServiceUnavailable, message)
        {
        }

        public GoodreadsException(string message, params object[] args)
            : base(HttpStatusCode.ServiceUnavailable, message, args)
        {
        }
    }
}
