using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.RQBit;
using NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

namespace NzbDrone.Core.Download.Clients.RQBit
{
    public interface IRQbitProxy
    {
        bool IsApiSupported(RQbitSettings settings);
        string GetVersion(RQbitSettings settings);
        List<RQBitTorrent> GetTorrents(RQbitSettings settings);
        void RemoveTorrent(string hash, bool removeData, RQbitSettings settings);
        string AddTorrentFromUrl(string torrentUrl, RQbitSettings settings);
        string AddTorrentFromFile(string fileName, byte[] fileContent, RQbitSettings settings);
        void SetTorrentLabel(string hash, string label, RQbitSettings settings);
        bool HasHashTorrent(string hash, RQbitSettings settings);
    }

    public class RQbitProxy : IRQbitProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public RQbitProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool IsApiSupported(RQbitSettings settings)
        {
            var request = BuildRequest(settings).Resource("");
            request.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Get(request.Build());

                // Check if we can connect and get a valid response
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var rootResponse = JsonConvert.DeserializeObject<RootResponse>(response.Content);
                    return !string.IsNullOrWhiteSpace(rootResponse?.Version);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "RQBit API not supported or not reachable");
                return false;
            }
        }

        public string GetVersion(RQbitSettings settings)
        {
            var version = "";
            var request = BuildRequest(settings).Resource("");
            var response = _httpClient.Get(request.Build());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var rootResponse = JsonConvert.DeserializeObject<RootResponse>(response.Content);
                version = rootResponse.Version;
            }
            else
            {
                _logger.Error("Failed to get torrent version");
            }

            return version;
        }

        public List<RQBitTorrent> GetTorrents(RQbitSettings settings)
        {
            var result = new List<RQBitTorrent>();
            var request = BuildRequest(settings).Resource("/torrents?with_stats=true");
            var response = _httpClient.Get(request.Build());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Error("Failed to get torrent list with stats");
                return result;
            }

            var torrentListWithStats = JsonConvert.DeserializeObject<TorrentWithStatsListResponse>(response.Content);

            if (torrentListWithStats?.Torrents != null)
            {
                foreach (var torrentWithStats in torrentListWithStats.Torrents)
                {
                    try
                    {
                        var torrent = new RQBitTorrent();
                        torrent.Id = torrentWithStats.Id;
                        torrent.Name = torrentWithStats.Name;
                        torrent.Hash = torrentWithStats.InfoHash;
                        torrent.TotalSize = torrentWithStats.Stats.TotalBytes;
                        torrent.Path = torrentWithStats.OutputFolder + torrentWithStats.Name;

                        var statsLive = torrentWithStats.Stats.Live;
                        if (statsLive?.DownloadSpeed != null)
                        {
                            // Convert mib/sec to bytes per second
                            torrent.DownRate = (long)(statsLive.DownloadSpeed.Mbps * 1048576);
                        }

                        torrent.RemainingSize = torrentWithStats.Stats.TotalBytes - torrentWithStats.Stats.ProgressBytes;

                        // Avoid division by zero
                        if (torrentWithStats.Stats.ProgressBytes > 0)
                        {
                            torrent.Ratio = (double)torrentWithStats.Stats.UploadedBytes / torrentWithStats.Stats.ProgressBytes;
                        }
                        else
                        {
                            torrent.Ratio = 0;
                        }

                        torrent.IsFinished = torrentWithStats.Stats.Finished;
                        torrent.IsActive = torrentWithStats.Stats.State != "paused";

                        result.Add(torrent);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to process torrent {0}", torrentWithStats.InfoHash);
                    }
                }
            }

            return result;
        }

        public void RemoveTorrent(string infoHash, bool removeData, RQbitSettings settings)
        {
            var endpoint = removeData ? "/delete" : "/forget";
            var itemRequest = BuildRequest(settings).Resource("/torrents/" + infoHash + endpoint);
            _httpClient.Post(itemRequest.Build());
        }

        public string AddTorrentFromUrl(string torrentUrl, RQbitSettings settings)
        {
            string infoHash = null;
            var itemRequest = BuildRequest(settings).Resource("/torrents?overwrite=true").Post().Build();
            itemRequest.SetContent(torrentUrl);
            var httpResponse = _httpClient.Post(itemRequest);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return infoHash;
            }

            var response = JsonConvert.DeserializeObject<PostTorrentResponse>(httpResponse.Content);

            if (response.Details != null)
            {
                infoHash = response.Details.InfoHash;
            }

            return infoHash;
        }

        public string AddTorrentFromFile(string fileName, byte[] fileContent, RQbitSettings settings)
        {
            string infoHash = null;
            var itemRequest = BuildRequest(settings)
                .Post()
                .Resource("/torrents?overwrite=true")
                .Build();
            itemRequest.SetContent(fileContent);
            var httpResponse = _httpClient.Post(itemRequest);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return infoHash;
            }

            var response = JsonConvert.DeserializeObject<PostTorrentResponse>(httpResponse.Content);

            if (response.Details != null)
            {
                infoHash = response.Details.InfoHash;
            }

            return infoHash;
        }

        public void SetTorrentLabel(string hash, string label, RQbitSettings settings)
        {
            _logger.Warn("Torrent labels currently unsupported by RQBit");
        }

        public bool HasHashTorrent(string hash, RQbitSettings settings)
        {
            var result = true;
            var rqBitTorrentResponse = GetTorrent(hash, settings);
            if (rqBitTorrentResponse == null || string.IsNullOrWhiteSpace(rqBitTorrentResponse.InfoHash))
            {
                result = false;
            }

            return result;
        }

        private TorrentResponse GetTorrent(string infoHash, RQbitSettings settings)
        {
            TorrentResponse result = null;
            var itemRequest = BuildRequest(settings).Resource("/torrents/" + infoHash);
            var itemResponse = _httpClient.Get(itemRequest.Build());
            if (itemResponse.StatusCode != HttpStatusCode.OK)
            {
                return result;
            }

            result = JsonConvert.DeserializeObject<TorrentResponse>(itemResponse.Content);

            return result;
        }

        private HttpRequestBuilder BuildRequest(RQbitSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase)
            {
                LogResponseContent = true,
            };
            return requestBuilder;
        }
    }
}
