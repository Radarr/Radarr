using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Filmweb.User
{
    public class FilmwebUserRequestGenerator : IImportListRequestGenerator
    {
        public FilmwebUserSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();
            pageableRequests.Add(GetMoviesRequest());
            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var requestBuilder = new HttpRequestBuilder(Settings.Link.Trim())
                .Accept(HttpAccept.Json);

            switch (Settings.FilmwebListType)
            {
                case (int)FilmwebUserListType.WantToSee:
                    requestBuilder.Resource($"/api/v1/user/{Settings.Username.Trim()}/want2see/film");
                    break;
                case (int)FilmwebUserListType.Rated:
                    requestBuilder.Resource($"/api/v1/user/{Settings.Username.Trim()}/votes/film");
                    break;
                case (int)FilmwebUserListType.Favorites:
                    requestBuilder.Resource($"/api/v1/user/{Settings.Username.Trim()}/favorites/film");
                    break;
            }

            var request = new ImportListRequest(requestBuilder.Build());
            yield return request;
        }
    }
}
