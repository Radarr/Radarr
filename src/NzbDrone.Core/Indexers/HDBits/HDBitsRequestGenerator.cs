using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(new TorrentQuery()));
            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var query = new TorrentQuery();

            if (TryAddSearchParameters(query, searchCriteria))
            {
                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        private bool TryAddSearchParameters(TorrentQuery query, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Movie.ImdbId.IsNullOrWhiteSpace())
            {
                return false;
            }

            var imdbId = int.Parse(searchCriteria.Movie.ImdbId.Substring(2));

            if (imdbId != 0)
            {
                query.ImdbInfo = query.ImdbInfo ?? new ImdbInfo();
                query.ImdbInfo.Id = imdbId;
                return true;
            }

            return false;
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private IEnumerable<IndexerRequest> GetRequest(TorrentQuery query)
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/api/torrents")
                .Build();

            request.Method = HttpMethod.Post;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            query.Category = Settings.Categories.ToArray();
            query.Codec = Settings.Codecs.ToArray();
            query.Medium = Settings.Mediums.ToArray();

            request.SetContent(query.ToJson());

            yield return new IndexerRequest(request);
        }
    }
}
