using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using System;
using System.Linq;
using System.Xml;

namespace NzbDrone.Core.Indexers.AwesomeHD
{
    public class AwesomeHDRssParser : IParseIndexerResponse
    {
        private readonly AwesomeHDSettings _settings;

        public AwesomeHDRssParser(AwesomeHDSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            // Hacky ¯\_(ツ)_/¯
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(indexerResponse.Content);

            var json = JsonConvert.SerializeXmlNode(doc);

            Console.WriteLine(json);

            var jsonResponse = JsonConvert.DeserializeObject<AwesomeHDSearchResponse>(json);

            if (jsonResponse == null)
            {
                throw new IndexerException(indexerResponse, "Unexpected response from request");
            }

            foreach (var torrent in jsonResponse.SearchResults.Torrent)
            {
                var id = torrent.Id;
                var title = $"{torrent.Name}.{torrent.Year}.{torrent.Resolution}.{torrent.Media}.{torrent.Encoding}.{torrent.Audioformat}-{torrent.Releasegroup}";

                torrentInfos.Add(new TorrentInfo()
                {
                    Guid = string.Format("AwesomeHD-{0}", id),
                    Title = title,
                    Size = torrent.Size,
                    DownloadUrl = GetDownloadUrl(id, jsonResponse.SearchResults.AuthKey, _settings.Passkey),
                    InfoUrl = GetInfoUrl(torrent.GroupId, id),
                    Seeders = int.Parse(torrent.Seeders),
                    Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                    PublishDate = torrent.Time.ToUniversalTime()
                });
            }

            return torrentInfos.OrderByDescending(o => ((dynamic)o).Seeders).ToArray();
        }

        private string GetDownloadUrl(string torrentId, string authKey, string passKey)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("action", "download")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("authkey", authKey)
                .AddQueryParam("torrent_pass", passKey);

            return url.FullUri;
        }

        private string GetInfoUrl(string groupId, string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("id", groupId)
                .AddQueryParam("torrentid", torrentId);

            return url.FullUri;
        }
    }
}
