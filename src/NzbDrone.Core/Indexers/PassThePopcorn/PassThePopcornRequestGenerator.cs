using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using RestSharp;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornRequestGenerator : IIndexerRequestGenerator
    {
        public PassThePopcornSettings Settings { get; set; }

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

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            var request =
                new IndexerRequest(
                    $"{Settings.BaseUrl.Trim().TrimEnd('/')}/torrents.php?json=noredirect&searchstr={searchParameters}",
                    HttpAccept.Json);

            var cookies = GetPTPCookies();
            foreach (var cookie in cookies)
            {
                request.HttpRequest.Cookies[cookie.Name] = cookie.Value;
            }

            yield return request;
        }

        private IList<RestResponseCookie> GetPTPCookies()
        {
            var client = new RestClient(Settings.BaseUrl.Trim().TrimEnd('/'));
            var request = new RestRequest("/ajax.php?action=login", Method.POST);
            request.AddParameter("username", Settings.Username);
            request.AddParameter("password", Settings.Password);
            request.AddParameter("passkey", Settings.Passkey);
            request.AddParameter("keeplogged", "1");
            request.AddParameter("login", "Log In!");
            request.AddHeader("Content-Type", "multipart/form-data");

            IRestResponse response = client.Execute(request);
            var content = response.Content;

            var jsonResponse = JObject.Parse(content);
            if (content != null && (string)jsonResponse["Result"] != "Error")
            {
                return response.Cookies;
            }

            throw new Exception("Error connecting to PTP invalid username, password, or passkey");
        }

    }
}
