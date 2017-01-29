using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport
{
    public class NetImportResponse
    {
        private readonly NetImportRequest _netImport;
        private readonly HttpResponse _httpResponse;

        public NetImportResponse(NetImportRequest netImport, HttpResponse httpResponse)
        {
            _netImport = netImport;
            _httpResponse = httpResponse;
        }

        public NetImportRequest Request => _netImport;

        public HttpRequest HttpRequest => _httpResponse.Request;

        public HttpResponse HttpResponse => _httpResponse;

        public string Content => _httpResponse.Content;
    }
}
