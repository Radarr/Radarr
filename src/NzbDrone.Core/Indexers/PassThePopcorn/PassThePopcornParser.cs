using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornParser : IParseIndexerResponse
    {
        private readonly PassThePopcornSettings _settings;
        private readonly Logger _logger;
        public PassThePopcornParser(PassThePopcornSettings settings, Logger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, $"Unexpected response status {indexerResponse.HttpResponse.StatusCode} code from API request");
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != HttpAccept.Json.Value)
            {
                throw new IndexerException(indexerResponse, $"Unexpected response header {indexerResponse.HttpResponse.Headers.ContentType} from API request, expected {HttpAccept.Json.Value}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<PassThePopcornResponse>(indexerResponse.Content);
            if (jsonResponse.TotalResults == "0" ||
                jsonResponse.TotalResults.IsNullOrWhiteSpace() ||
                jsonResponse.Movies == null)
            {
                return torrentInfos;
            }

            foreach (var result in jsonResponse.Movies)
            {
                foreach (var torrent in result.Torrents)
                {
                    var id = torrent.Id;
                    IndexerFlags flags = 0;

                    if (torrent.GoldenPopcorn)
                    {
                        flags |= IndexerFlags.PTP_Golden;
                    }

                    if (torrent.Checked)
                    {
                        flags |= IndexerFlags.PTP_Approved;
                    }

                    if (torrent.FreeleechType == "Freeleech")
                    {
                        flags |= IndexerFlags.G_Freeleech;
                    }

                    if (torrent.Scene)
                    {
                        flags |= IndexerFlags.G_Scene;
                    }

                    // Only add approved torrents
                    try
                    {
                        torrentInfos.Add(new PassThePopcornInfo
                        {
                            Guid = $"PassThePopcorn-{id}",
                            Title = torrent.ReleaseName,
                            Size = long.Parse(torrent.Size),
                            DownloadUrl = GetDownloadUrl(id, jsonResponse.AuthKey, jsonResponse.PassKey),
                            InfoUrl = GetInfoUrl(result.GroupId, id),
                            Seeders = int.Parse(torrent.Seeders),
                            Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                            PublishDate = TimeZoneInfo.ConvertTimeToUtc(torrent.UploadTime, TimeZoneInfo.Utc), // PTP returns UTC timestamps, without a timezone specifier.
                            Golden = torrent.GoldenPopcorn,
                            Scene = torrent.Scene,
                            Approved = torrent.Checked,
                            ImdbId = result.ImdbId.IsNotNullOrWhiteSpace() ? int.Parse(result.ImdbId) : 0,
                            IndexerFlags = flags
                        });
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Encountered exception parsing PTP torrent: {" +
                                         $"Size: {torrent.Size}" +
                                         $"UploadTime: {torrent.UploadTime}" +
                                         $"Seeders: {torrent.Seeders}" +
                                         $"Leechers: {torrent.Leechers}" +
                                         $"ReleaseName: {torrent.ReleaseName}" +
                                         $"ID: {torrent.Id}" +
                                         "}. Please immediately report this info on https://github.com/Radarr/Radarr/issues/new?assignees=&labels=bug&template=bug_report.md&title=Encountered%20Exception%20Parsing%20%20PTP%20Torrent.");
                        throw;
                    }
                }
            }

            return
                torrentInfos;
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

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
