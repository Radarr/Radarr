using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListResponse
    {
        private readonly ImportListRequest _importList;
        private readonly HttpResponse _httpResponse;

        public ImportListResponse(ImportListRequest importList, HttpResponse httpResponse)
        {
            _importList = importList;
            _httpResponse = httpResponse;
        }

        public ImportListRequest Request => _importList;

        public HttpRequest HttpRequest => _httpResponse.Request;

        public HttpResponse HttpResponse => _httpResponse;

        public string Content => _httpResponse.Content;
    }
}
