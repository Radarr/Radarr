using System;
using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Common.Cache;
using NLog;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornRequestGenerator : IIndexerRequestGenerator
    {

        public PassThePopcornSettings Settings { get; set; }

        public IDictionary<string, string> Cookies { get; set; }

        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(null));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(searchCriteria.Movie.ImdbId));
            return pageableRequests;
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            Cookies = GetCookies();
            
            Authenticate();

            var request =
                new IndexerRequest(
                    $"{Settings.BaseUrl.Trim().TrimEnd('/')}/torrents.php?action=advanced&json=noredirect&searchstr={searchParameters}",
                    HttpAccept.Json);

            foreach (var cookie in Cookies)
            {
                request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
            }

            CookiesUpdater(Cookies, DateTime.Now + TimeSpan.FromDays(30));

            yield return request;
        }

        private void Authenticate()
        {
            if (Cookies == null)
            {
                var requestBuilder = new HttpRequestBuilder($"{Settings.BaseUrl.Trim().TrimEnd('/')}")
                {
                    LogResponseContent = true
                };

                requestBuilder.Method = HttpMethod.POST;
                requestBuilder.Resource("ajax.php?action=login");
                requestBuilder.PostProcess += r => r.RequestTimeout = TimeSpan.FromSeconds(15);
                
                var authLoginRequest = requestBuilder
                    .AddFormParameter("username", Settings.Username)
                    .AddFormParameter("password", Settings.Password)
                    .AddFormParameter("passkey", Settings.Passkey)
                    .AddFormParameter("keeplogged", "1")
                    .SetHeader("Content-Type", "multipart/form-data")
                    .Accept(HttpAccept.Json)
                    .Build();

                authLoginRequest.AllowAutoRedirect = true;
                // We want clean cookies for the auth request.
                authLoginRequest.StoreRequestCookie = false;
                authLoginRequest.StoreResponseCookie = false;
                authLoginRequest.Cookies.Clear();
                authLoginRequest.IgnorePersistentCookies = true;
                var response = HttpClient.Execute(authLoginRequest);
                var result = Json.Deserialize<PassThePopcornAuthResponse>(response.Content);

                if (result?.Result != "Ok" || string.IsNullOrWhiteSpace(result.Result))
                {
                    Logger.Debug("PassThePopcorn authentication failed.");
                    throw new Exception("Failed to authenticate with PassThePopcorn.");
                }

                Logger.Debug("PassThePopcorn authentication succeeded.");

                Cookies = response.GetCookies();
                requestBuilder.SetCookies(Cookies);
            }
        }
    }
}
