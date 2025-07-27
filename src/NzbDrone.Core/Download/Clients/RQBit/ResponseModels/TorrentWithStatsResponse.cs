using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels
{
    public class TorrentWithStatsListResponse
    {
        [JsonProperty("torrents")]
        public List<TorrentWithStatsResponse> Torrents { get; set; }
    }

    public class TorrentWithStatsResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("info_hash")]
        public string InfoHash { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("output_folder")]
        public string OutputFolder { get; set; }

        [JsonProperty("stats")]
        public TorrentStatsResponse Stats { get; set; }
    }

    public class TorrentStatsResponse
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("file_progress")]
        public List<long> FileProgress { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("progress_bytes")]
        public long ProgressBytes { get; set; }

        [JsonProperty("uploaded_bytes")]
        public long UploadedBytes { get; set; }

        [JsonProperty("total_bytes")]
        public long TotalBytes { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }

        [JsonProperty("live")]
        public TorrentLiveStatsResponse Live { get; set; }
    }

    public class TorrentLiveStatsResponse
    {
        [JsonProperty("snapshot")]
        public TorrentSnapshotResponse Snapshot { get; set; }

        [JsonProperty("download_speed")]
        public TorrentSpeedResponse DownloadSpeed { get; set; }

        [JsonProperty("upload_speed")]
        public TorrentSpeedResponse UploadSpeed { get; set; }

        [JsonProperty("time_remaining")]
        public TorrentTimeRemainingResponse TimeRemaining { get; set; }
    }

    public class TorrentSnapshotResponse
    {
        [JsonProperty("downloaded_and_checked_bytes")]
        public long DownloadedAndCheckedBytes { get; set; }

        [JsonProperty("fetched_bytes")]
        public long FetchedBytes { get; set; }

        [JsonProperty("uploaded_bytes")]
        public long UploadedBytes { get; set; }

        [JsonProperty("peer_stats")]
        public TorrentPeerStatsResponse PeerStats { get; set; }
    }

    public class TorrentSpeedResponse
    {
        [JsonProperty("mbps")]
        public double Mbps { get; set; }

        [JsonProperty("human_readable")]
        public string HumanReadable { get; set; }
    }

    public class TorrentTimeRemainingResponse
    {
        [JsonProperty("duration")]
        public TorrentDurationResponse Duration { get; set; }

        [JsonProperty("human_readable")]
        public string HumanReadable { get; set; }
    }

    public class TorrentDurationResponse
    {
        [JsonProperty("secs")]
        public long Secs { get; set; }

        [JsonProperty("nanos")]
        public long Nanos { get; set; }
    }

    public class TorrentPeerStatsResponse
    {
        [JsonProperty("queued")]
        public int Queued { get; set; }

        [JsonProperty("connecting")]
        public int Connecting { get; set; }

        [JsonProperty("live")]
        public int Live { get; set; }

        [JsonProperty("seen")]
        public int Seen { get; set; }

        [JsonProperty("dead")]
        public int Dead { get; set; }

        [JsonProperty("not_needed")]
        public int NotNeeded { get; set; }

        [JsonProperty("steals")]
        public int Steals { get; set; }
    }
} 
