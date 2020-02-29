using Nancy;
using Readarr.Http.Exceptions;

namespace Readarr.Http.REST
{
    public class UnsupportedMediaTypeException : ApiException
    {
        public UnsupportedMediaTypeException(object content = null)
            : base(HttpStatusCode.UnsupportedMediaType, content)
        {
        }
    }
}
