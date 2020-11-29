using System;
using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.AirDCPP.Responses;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPParser : IParseIndexerResponse
    {
        public AirDCPPParser()
        {
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeExtensions.Epoch.AddSeconds(unixTime);
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var releases = new List<ReleaseInfo>();

            if (indexerResponse != null)
            {
                // here we receive the response from the search we defined in the request generator
                if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new IndexerException(indexerResponse,
                        "Unexpected response status {0} code from API request",
                        indexerResponse.HttpResponse.StatusCode);
                }

                var splitUrl = indexerResponse.HttpRequest.Url.Path.Split('/');
                var searchInstanceId = splitUrl[4];

                var searchResults = Json.Deserialize<List<SearchResult>>(indexerResponse.Content);

                foreach (var searchResult in searchResults)
                {
                    releases.Add(new ReleaseInfo()
                    {
                        Guid = $"AirDCPP-{searchResult.id}",
                        Title = searchResult.name,
                        Size = searchResult.size,
                        DownloadUrl = $"{searchInstanceId}:{searchResult.id}",
                        PublishDate = searchResult.time.HasValue ? FromUnixTime(searchResult.time.Value) : DateTime.Now,
                        Source = "AirDC++",
                        DownloadProtocol = DownloadProtocol.DirectConnect
                    });
                }
            }

            return releases.ToArray();
        }
    }
}
