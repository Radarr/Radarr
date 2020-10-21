using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Simkl;

namespace NzbDrone.Core.ImportLists.Simkl.List
{
    public class SimklListRequestGenerator : IImportListRequestGenerator
    {
        private readonly ISimklProxy _simklProxy;
        public SimklListSettings Settings { get; set; }

        public SimklListRequestGenerator(ISimklProxy simklProxy)
        {
            _simklProxy = simklProxy;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var link = string.Empty;

            var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
            link += $"users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";

            var request = new ImportListRequest(_simklProxy.BuildSimklRequest(link, HttpMethod.GET, Settings.AccessToken));

            yield return request;
        }
    }
}
