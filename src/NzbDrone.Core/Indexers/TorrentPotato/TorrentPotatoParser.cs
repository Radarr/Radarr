using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TorrentPotato
{
    public class TorrentPotatoParser : IParseIndexerResponse
    {
        private static readonly Regex RegexGuid = new Regex(@"^magnet:\?xt=urn:btih:([a-f0-9]+)", RegexOptions.Compiled);

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();

            switch (indexerResponse.HttpResponse.StatusCode)
            {
                default:
                    if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
                    }

                    break;
            }

            var jsonResponse = new HttpResponse<TorrentPotatoResponse>(indexerResponse.HttpResponse);

            foreach (var torrent in jsonResponse.Resource.results)
            {
                var torrentInfo = new TorrentInfo();

                torrentInfo.Guid = GetGuid(torrent);
                torrentInfo.Title = torrent.release_name;
                torrentInfo.Size = (long)torrent.size * 1000 * 1000;
                torrentInfo.DownloadUrl = torrent.download_url;
                torrentInfo.InfoUrl = torrent.details_url;
                torrentInfo.PublishDate = torrent.publish_date.ToUniversalTime();
                torrentInfo.Seeders = torrent.seeders;
                torrentInfo.Peers = torrent.leechers + torrent.seeders;

                results.Add(torrentInfo);
            }

            return results;
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private string GetGuid(Result torrent)
        {
            var match = RegexGuid.Match(torrent.download_url);

            if (match.Success)
            {
                return string.Format("potato-{0}", match.Groups[1].Value);
            }
            else
            {
                return string.Format("potato-{0}", torrent.download_url);
            }
        }
    }
}
