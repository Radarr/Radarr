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

        public ICached<Dictionary<string, string>> AuthCookieCache { get; set; }
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

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            Authenticate();

            var filter = "";
            if (searchParameters == null)
            {


            }

            var request =
                new IndexerRequest(
                    $"{Settings.BaseUrl.Trim().TrimEnd('/')}/torrents.php?action=advanced&json=noredirect&searchstr={searchParameters}{filter}",
                    HttpAccept.Json);

            var cookies = AuthCookieCache.Find(Settings.BaseUrl.Trim().TrimEnd('/'));
            foreach (var cookie in cookies)
            {
                request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
            }

            yield return request;
        }

        private void Authenticate()
        {
            var requestBuilder = new HttpRequestBuilder($"{Settings.BaseUrl.Trim().TrimEnd('/')}")
            {
                LogResponseContent = true
            };

            requestBuilder.Method = HttpMethod.POST;
            requestBuilder.Resource("ajax.php?action=login");
            requestBuilder.PostProcess += r => r.RequestTimeout = TimeSpan.FromSeconds(15);

            var authKey = Settings.BaseUrl.Trim().TrimEnd('/');
            var cookies = AuthCookieCache.Find(authKey);

            if (cookies == null)
            {
                AuthCookieCache.Remove(authKey);
                var authLoginRequest = requestBuilder
                    .AddFormParameter("username", Settings.Username)
                    .AddFormParameter("password", Settings.Password)
                    .AddFormParameter("passkey", Settings.Passkey)
                    .AddFormParameter("keeplogged", "1")
                    .AddFormParameter("login", "Log In!")
                    .SetHeader("Content-Type", "multipart/form-data")
                    .Accept(HttpAccept.Json)
                    .Build();

                var response = HttpClient.Execute(authLoginRequest);
                var result = Json.Deserialize<PassThePopcornAuthResponse>(response.Content);

                if (result.Result != "Ok" || string.IsNullOrWhiteSpace(result.Result))
                {
                    Logger.Debug("PassThePopcorn authentication failed.");
                    throw new Exception("Failed to authenticate with PassThePopcorn.");
                }

                Logger.Debug("PassThePopcorn authentication succeeded.");

                cookies = response.GetCookies();
                AuthCookieCache.Set(authKey, cookies, new TimeSpan(7, 0, 0, 0, 0)); // re-auth every 7 days
                requestBuilder.SetCookies(cookies);
            }
            else
            {
                requestBuilder.SetCookies(cookies);
            }
        }
    }
}
