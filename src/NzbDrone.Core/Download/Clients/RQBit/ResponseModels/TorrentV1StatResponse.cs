using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentV1StatResponse
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
    public TorrentV1StatLiveResponse Live { get; set; }
}

public class RQBitTorrentSpeedResponse
{
    [JsonProperty("mbps")]
    public long Mbps { get; set; }

    [JsonProperty("human_readable")]
    public string HumanReadable { get; set; }
}

public class TorrentV1StatLiveResponse
{
    [JsonProperty("snapshot")]
    public TorrentV1StatLiveSnapshotResponse Snapshot { get; set; }
    [JsonProperty("download_speed")]
    public RQBitTorrentSpeedResponse DownloadSpeed { get; set; }
    [JsonProperty("upload_speed")]
    public RQBitTorrentSpeedResponse UploadSpeed { get; set; }
}

public class TorrentV1StatLiveSnapshotResponse
{
    [JsonProperty("downloaded_and_checked_bytes")]
    public long DownloadedAndCheckedBytes { get; set; }

    [JsonProperty("fetched_bytes")]
    public long FetchedBytes { get; set; }

    [JsonProperty("uploaded_bytes")]
    public long UploadedBytes { get; set; }

    [JsonProperty("downloaded_and_checked_pieces")]
    public long DownloadedAndCheckedPieces { get; set; }

    [JsonProperty("total_piece_downloaded_ms")]
    public long TotalPieceDownloadedMs { get; set; }

    [JsonProperty("peer_stats")]
    public TorrentV1StatLiveSnapshotPeerStatsResponse PeerStats { get; set; }
}

public class TorrentV1StatLiveSnapshotPeerStatsResponse
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
