using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using System;
using System.Linq;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornParser : IParseIndexerResponse
    {
        private readonly PassThePopcornSettings _settings;

        public PassThePopcornParser(PassThePopcornSettings settings)
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

            var jsonResponse = JsonConvert.DeserializeObject<PassThePopcornResponse>(indexerResponse.Content);

            var responseData = jsonResponse.Movies;
            if (responseData == null)
            {
                throw new IndexerException(indexerResponse,
                    "Indexer API call response missing result data");
            }

            foreach (var result in responseData)
            {
                foreach (var torrent in result.Torrents)
                {
                    var id = torrent.Id;
                    var title = torrent.ReleaseName;

                    if (torrent.GoldenPopcorn)
                    {
                        title = $"{title} ★";
                    }

                    if (torrent.Checked)
                    {
                        title = $"{title} ✔";
                    }

                    //if (IsPropertyExist(torrent, "RemasterTitle"))
                    //{
                    //    if (torrent.RemasterTitle != null)
                    //    {
                    //        title = $"{title} - {torrent.RemasterTitle}";
                    //    }
                    //}

                    // Only add approved torrents
                    if (_settings.Approved && torrent.Checked)
                    {
                        torrentInfos.Add(new TorrentInfo()
                        {
                            Guid = string.Format("PassThePopcorn-{0}", id),
                            Title = title,
                            Size = long.Parse(torrent.Size),
                            DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                            InfoUrl = GetInfoUrl(result.GroupId, id),
                            Seeders = int.Parse(torrent.Seeders),
                            Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                            PublishDate = torrent.UploadTime.ToUniversalTime(),
                            Golden = torrent.GoldenPopcorn,
                            Scene = torrent.Scene,
                            Approved = torrent.Checked
                        });
                    }
                    // Add all torrents
                    else if (!_settings.Approved)
                    {
                        torrentInfos.Add(new TorrentInfo()
                        {
                            Guid = string.Format("PassThePopcorn-{0}", id),
                            Title = title,
                            Size = long.Parse(torrent.Size),
                            DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                            InfoUrl = GetInfoUrl(result.GroupId, id),
                            Seeders = int.Parse(torrent.Seeders),
                            Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                            PublishDate = torrent.UploadTime.ToUniversalTime(),
                            Golden = torrent.GoldenPopcorn,
                            Scene = torrent.Scene,
                            Approved = torrent.Checked
                        });
                    }
                    // Don't add any torrents
                    else if (_settings.Approved && !torrent.Checked)
                    {
                        continue;
                    }
                }
            }

            // prefer golden
            // prefer scene
            // require approval
            return torrentInfos.OrderBy(o => ((dynamic)o).Golden ? 0 : 1).ThenBy(o => ((dynamic)o).Scene ? 0 : 1).ThenByDescending(o => ((dynamic)o).PublishDate).ToArray();
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

        //public static bool IsPropertyExist(dynamic torrents, string name)
        //{
        //    return torrents.GetType().GetProperty(name) != null;
        //}
    }
}
