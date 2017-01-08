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

                    if (_settings.GoldenOnly)
                    {
                        if (torrent.GoldenPopcorn)
                        {
                            torrentInfos.Add(new TorrentInfo()
                            {
                                Guid = string.Format("PassThePopcorn-{0}", id),
                                Title = torrent.ReleaseName,
                                Size = Int64.Parse(torrent.Size),
                                DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                                InfoUrl = GetInfoUrl(result.GroupId, id),
                                Seeders = Int32.Parse(torrent.Seeders),
                                Peers = Int32.Parse(torrent.Leechers) + Int32.Parse(torrent.Seeders),
                                PublishDate = torrent.UploadTime.ToUniversalTime(),
                                Golden = torrent.GoldenPopcorn,
                                Checked = torrent.Checked
                            });
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (_settings.CheckedOnly)
                    {
                        if (torrent.Checked)
                        {
                            torrentInfos.Add(new TorrentInfo()
                            {
                                Guid = string.Format("PassThePopcorn-{0}", id),
                                Title = torrent.ReleaseName,
                                Size = Int64.Parse(torrent.Size),
                                DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                                InfoUrl = GetInfoUrl(result.GroupId, id),
                                Seeders = Int32.Parse(torrent.Seeders),
                                Peers = Int32.Parse(torrent.Leechers) + Int32.Parse(torrent.Seeders),
                                PublishDate = torrent.UploadTime.ToUniversalTime(),
                                Golden = torrent.GoldenPopcorn,
                                Checked = torrent.Checked
                            });
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!_settings.GoldenOnly && !_settings.CheckedOnly)
                    {
                        torrentInfos.Add(new TorrentInfo()
                        {
                            Guid = string.Format("PassThePopcorn-{0}", id),
                            Title = torrent.ReleaseName,
                            Size = Int64.Parse(torrent.Size),
                            DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                            InfoUrl = GetInfoUrl(result.GroupId, id),
                            Seeders = Int32.Parse(torrent.Seeders),
                            Peers = Int32.Parse(torrent.Leechers) + Int32.Parse(torrent.Seeders),
                            PublishDate = torrent.UploadTime.ToUniversalTime(),
                            Golden = torrent.GoldenPopcorn,
                            Checked = torrent.Checked
                        });
                    }
                }
            }

            return torrentInfos.OrderBy(o => ((dynamic)o).Golden ? 0 : 1).ThenBy(o => ((dynamic)o).Checked ? 0 : 1).ThenBy(o => ((dynamic)o).PublishDate).ToArray();
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
