using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.RQbit;
using NzbDrone.Core.Download.Clients.RQBit;
using NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

namespace NzbDrone.Core.Download.Clients.rQbit;

public interface IRQbitProxy
{
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

    public RQbitProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GetVersion(RQbitSettings settings)
    {
        var version = "";
        var request = BuildRequest(settings).Resource("");
        var response = _httpClient.Get(request.Build());
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonStr = Encoding.UTF8.GetString(response.ResponseData);
            var rootResponse = JsonConvert.DeserializeObject<RootResponse>(jsonStr);
            version = rootResponse.version;
        }
        else
        {
            _logger.Error("Failed to get torrent version");
        }

        return version;
    }

    public List<RQBitTorrent> GetTorrents(RQbitSettings settings)
    {
        List<RQBitTorrent> result = null;
        var request = BuildRequest(settings).Resource("/torrents");
        var response = _httpClient.Get(request.Build());
        TorrentListResponse torrentList = null;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonStr = Encoding.UTF8.GetString(response.ResponseData);
            torrentList = JsonConvert.DeserializeObject<TorrentListResponse>(jsonStr);
        }
        else
        {
            _logger.Error("Failed to get torrent version");
        }

        if (torrentList != null)
        {
            result = new List<RQBitTorrent>();
            foreach (var torrentListItem in torrentList.torrents)
            {
                var torrentResponse = getTorrent(torrentListItem.info_hash, settings);
                var torrentStatsResponse = getTorrentStats(torrentListItem.info_hash, settings);
                var torrent = new RQBitTorrent();

                torrent.id = torrentListItem.id;
                torrent.Name = torrentResponse.name;
                torrent.Hash = torrentResponse.info_hash;
                torrent.TotalSize = torrentStatsResponse.total_bytes;

                var statsLive = torrentStatsResponse.live;
                if (statsLive != null && statsLive.snapshot != null)
                {
                    torrent.DownRate = statsLive.download_speed.mbps * 1048576; // mib/sec -> bytes per second
                }

                torrent.RemainingSize = torrentStatsResponse.total_bytes - torrentStatsResponse.progress_bytes;
                torrent.Ratio = torrentStatsResponse.uploaded_bytes / torrentStatsResponse.progress_bytes;
                torrent.IsFinished = torrentStatsResponse.finished;
                torrent.IsActive = torrentStatsResponse.state != "paused";

                result.Add(torrent);
            }
        }

        return result;
    }

    public void RemoveTorrent(string info_hash, bool removeData, RQbitSettings settings)
    {
        var endpoint = removeData ? "/delete" :  "/forget";
        var itemRequest = BuildRequest(settings).Resource("/torrents/" + info_hash + endpoint);
        _httpClient.Post(itemRequest.Build());
    }

    public string AddTorrentFromUrl(string torrentUrl, RQbitSettings settings)
    {
        string info_hash = null;
        var itemRequest = BuildRequest(settings).Resource("/torrents?overwrite=true").Post().Build();
        itemRequest.SetContent(torrentUrl);
        var httpResponse = _httpClient.Post(itemRequest);
        if (httpResponse.StatusCode != HttpStatusCode.OK)
        {
            return info_hash;
        }

        var jsonStr = Encoding.UTF8.GetString(httpResponse.ResponseData);
        var response = JsonConvert.DeserializeObject<PostTorrentResponse>(jsonStr);

        if (response.details != null)
        {
            info_hash = response.details.info_hash;
        }

        return info_hash;
    }

    public string AddTorrentFromFile(string fileName, byte[] fileContent, RQbitSettings settings)
    {
        string info_hash = null;
        var itemRequest = BuildRequest(settings)
            .Post()
            .Resource("/torrents?overwrite=true")
            .Build();
        itemRequest.SetContent(fileContent);
        var httpResponse = _httpClient.Post(itemRequest);
        if (httpResponse.StatusCode != HttpStatusCode.OK)
        {
            return info_hash;
        }

        var jsonStr = Encoding.UTF8.GetString(httpResponse.ResponseData);
        var response = JsonConvert.DeserializeObject<PostTorrentResponse>(jsonStr);

        if (response.details != null)
        {
            info_hash = response.details.info_hash;
        }

        return info_hash;
    }

    public void SetTorrentLabel(string hash, string label, RQbitSettings settings)
    {
        _logger.Warn("Torrent labels currently unsupported by RQBit");
    }

    public bool HasHashTorrent(string hash, RQbitSettings settings)
    {
        var result = true;
        var rqBitTorrentResponse = getTorrent(hash, settings);
        if (rqBitTorrentResponse == null || string.IsNullOrWhiteSpace(rqBitTorrentResponse.info_hash))
        {
            result = false;
        }

        return result;
    }

    private TorrentResponse getTorrent(string info_hash, RQbitSettings settings)
    {
        TorrentResponse result = null;
        var itemRequest = BuildRequest(settings).Resource("/torrents/" + info_hash);
        var itemResponse = _httpClient.Get(itemRequest.Build());
        if (itemResponse.StatusCode != HttpStatusCode.OK)
        {
            return result;
        }

        var jsonStr = Encoding.UTF8.GetString(itemResponse.ResponseData);
        result = JsonConvert.DeserializeObject<TorrentResponse>(jsonStr);

        return result;
    }

    private TorrentV1StatResponse getTorrentStats(string info_hash, RQbitSettings settings)
    {
        TorrentV1StatResponse result = null;
        var itemRequest = BuildRequest(settings).Resource("/torrents/" + info_hash + "/stats/v1");
        var itemResponse = _httpClient.Get(itemRequest.Build());
        if (itemResponse.StatusCode != HttpStatusCode.OK)
        {
            return result;
        }

        var jsonStr = Encoding.UTF8.GetString(itemResponse.ResponseData);
        result = JsonConvert.DeserializeObject<TorrentV1StatResponse>(jsonStr);

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
