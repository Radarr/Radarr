using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentV1StatResponse
{
    public string state { get; set; }
    public List<long> file_progress { get; set; }
    public string error { get; set; }
    public long progress_bytes { get; set; }
    public long uploaded_bytes { get; set; }
    public long total_bytes { get; set; }
    public bool finished { get; set; }
    public TorrentV1StatLiveResponse live { get; set; }
}

public class RQBitTorrentSpeedResponse
{
    public long mbps { get; set; }
    public string human_readable { get; set; }
}

public class TorrentV1StatLiveResponse
{
    public TorrentV1StatLiveSnapshotResponse snapshot { get; set; }
    public RQBitTorrentSpeedResponse download_speed { get; set; }
    public RQBitTorrentSpeedResponse upload_speed { get; set; }
}

public class TorrentV1StatLiveSnapshotResponse
{
    public long downloaded_and_checked_bytes { get; set; }
    public long fetched_bytes { get; set; }
    public long uploaded_bytes { get; set; }
    public long downloaded_and_checked_pieces { get; set; }
    public long total_piece_downloaded_ms { get; set; }
    public TorrentV1StatLiveSnapshotPeerStatsResponse peer_stats { get; set; }
}

public class TorrentV1StatLiveSnapshotPeerStatsResponse
{
    public int queued { get; set; }
    public int connecting { get; set; }
    public int live { get; set; }
    public int seen { get; set; }
    public int dead { get; set; }
    public int not_needed { get; set; }
    public int steals { get; set; }
}
