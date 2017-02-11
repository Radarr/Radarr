using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport
{
    public class NetImportRequest
    {
        public HttpRequest HttpRequest { get; private set; }

        public NetImportRequest(string url, HttpAccept httpAccept)
        {
            HttpRequest = new HttpRequest(url, httpAccept);
        }

        public NetImportRequest(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public HttpUri Url => HttpRequest.Url;
    }
}
