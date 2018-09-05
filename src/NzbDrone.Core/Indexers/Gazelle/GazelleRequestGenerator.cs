using System;
using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Common.Cache;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers.Gazelle
{
    public class GazelleRequestGenerator : IIndexerRequestGenerator
    {

        public GazelleSettings Settings { get; set; }

        public ICached<Dictionary<string, string>> AuthCookieCache { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(null));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(string.Format("&artistname={0}&groupname={1}", searchCriteria.ArtistQuery, searchCriteria.AlbumQuery)));
            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(string.Format("&artistname={0}",searchCriteria.ArtistQuery)));
            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            Authenticate();

            var filter = "";
            if (searchParameters == null)
            {

            }

            var request =
                new IndexerRequest(
                    $"{Settings.BaseUrl.Trim().TrimEnd('/')}/ajax.php?action=browse&searchstr={searchParameters}{filter}",
                    HttpAccept.Json);

            var cookies = AuthCookieCache.Find(Settings.BaseUrl.Trim().TrimEnd('/'));
            foreach (var cookie in cookies)
            {
                request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
            }

            yield return request;
        }

        private GazelleAuthResponse GetIndex(Dictionary<string,string> cookies)
        {
            var indexRequestBuilder = new HttpRequestBuilder($"{Settings.BaseUrl.Trim().TrimEnd('/')}")
            {
                LogResponseContent = true
            };

            indexRequestBuilder.SetCookies(cookies);
            indexRequestBuilder.Method = HttpMethod.POST;
            indexRequestBuilder.Resource("ajax.php?action=index");

            var authIndexRequest = indexRequestBuilder
                .Accept(HttpAccept.Json)
                .Build();

            var indexResponse = HttpClient.Execute(authIndexRequest);

            var result = Json.Deserialize<GazelleAuthResponse>(indexResponse.Content);

            return result;
        }

        private void Authenticate()
        {

            var requestBuilder = new HttpRequestBuilder($"{Settings.BaseUrl.Trim().TrimEnd('/')}")
            {
                LogResponseContent = true
            };

            requestBuilder.Method = HttpMethod.POST;
            requestBuilder.Resource("login.php");
            requestBuilder.PostProcess += r => r.RequestTimeout = TimeSpan.FromSeconds(15);

            var authKey = Settings.BaseUrl.Trim().TrimEnd('/');
            var cookies = AuthCookieCache.Find(authKey);

            if (cookies == null)
            {
                AuthCookieCache.Remove(authKey);
                var authLoginRequest = requestBuilder
                    .AddFormParameter("username", Settings.Username)
                    .AddFormParameter("password", Settings.Password)
                    .AddFormParameter("keeplogged", "1")
                    .SetHeader("Content-Type", "multipart/form-data")
                    .Accept(HttpAccept.Json)
                    .Build();

                var response = HttpClient.Execute(authLoginRequest);

                cookies = response.GetCookies();

                AuthCookieCache.Set(authKey, cookies);
            }

            var index = GetIndex(cookies);

            if (index == null || index.Status.IsNullOrWhiteSpace() || index.Status != "success")
            {
                Logger.Debug("Gazelle authentication failed.");
                AuthCookieCache.Remove(authKey);
                throw new Exception("Failed to authenticate with Gazelle.");
            }

            Logger.Debug("Gazelle authentication succeeded.");

            Settings.AuthKey = index.Response.Authkey;
            Settings.PassKey = index.Response.Passkey;

        }        
    }
}
