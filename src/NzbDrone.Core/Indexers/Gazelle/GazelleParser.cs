using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Indexers.Gazelle
{
    public class GazelleParser : IParseIndexerResponse
    {
        private readonly GazelleSettings _settings;
        public ICached<Dictionary<string, string>> AuthCookieCache { get; set; }

        public GazelleParser(GazelleSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                // Remove cookie cache
                AuthCookieCache.Remove(_settings.BaseUrl.Trim().TrimEnd('/'));

                throw new IndexerException(indexerResponse, $"Unexpected response status {indexerResponse.HttpResponse.StatusCode} code from API request");
            }

            if (!indexerResponse.HttpResponse.Headers.ContentType.Contains(HttpAccept.Json.Value))
            {
                // Remove cookie cache
                AuthCookieCache.Remove(_settings.BaseUrl.Trim().TrimEnd('/'));

                throw new IndexerException(indexerResponse, $"Unexpected response header {indexerResponse.HttpResponse.Headers.ContentType} from API request, expected {HttpAccept.Json.Value}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<GazelleResponse>(WebUtility.HtmlDecode(indexerResponse.Content));
            if (jsonResponse.Status != "success" ||
                jsonResponse.Status.IsNullOrWhiteSpace() ||
                jsonResponse.Response == null)
            {
                return torrentInfos;
            }


            foreach (var result in jsonResponse.Response.Results)
            {
                if (result.Torrents != null)
                {
                    foreach (var torrent in result.Torrents)
                    {
                        var id = torrent.TorrentId;

                        torrentInfos.Add(new GazelleInfo()
                        {
                            Guid = string.Format("Gazelle-{0}", id),
                            Artist = result.Artist,
                            // Splice Title from info to avoid calling API again for every torrent.
                            Title = result.Artist + " - " + result.GroupName + " (" + result.GroupYear +") (" + torrent.Format + " " + torrent.Encoding + ")",
                            Album = result.GroupName,
                            Container = torrent.Encoding,
                            Codec = torrent.Format,
                            Size = long.Parse(torrent.Size),
                            DownloadUrl = GetDownloadUrl(id, _settings.AuthKey, _settings.PassKey),
                            InfoUrl = GetInfoUrl(result.GroupId, id),
                            Seeders = int.Parse(torrent.Seeders),
                            Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                            PublishDate = torrent.Time.ToUniversalTime(),
                            Scene = torrent.Scene,
                        });
                    }
                }
            }

            var torr = torrentInfos;
            // order by date
            return
                torrentInfos
                    .OrderByDescending(o => o.PublishDate)
                    .ToArray();

        }

        private string GetDownloadUrl(int torrentId, string authKey, string passKey)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("action", "download")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("authkey", authKey)
                .AddQueryParam("torrent_pass", passKey);

            return url.FullUri;
        }

        private string GetInfoUrl(string groupId, int torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("id", groupId)
                .AddQueryParam("torrentid", torrentId);

            return url.FullUri;
        }
    }
}
