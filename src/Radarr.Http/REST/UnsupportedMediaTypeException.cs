using System.Net;
using Radarr.Http.Exceptions;

namespace Radarr.Http.REST
{
    public class UnsupportedMediaTypeException : ApiException
    {
        public UnsupportedMediaTypeException(object content = null)
            : base(HttpStatusCode.UnsupportedMediaType, content)
        {
        }
    }
}
