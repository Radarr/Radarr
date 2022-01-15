using System;

namespace NzbDrone.Core.Download.Clients.QBittorrent;

public interface IQBittorrentStalledPolicy
{
    void Apply(QBittorrentTorrent torrent, DownloadClientItem ditem, QBittorrentSettings settings);
}

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}

public sealed class QBittorrentStalledPolicy : IQBittorrentStalledPolicy
{
    private readonly IDateTimeProvider _dateTime;

    public QBittorrentStalledPolicy(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public void Apply(QBittorrentTorrent torrent, DownloadClientItem ditem, QBittorrentSettings settings)
    {
        if (torrent.State != "stalledDL")
        {
            return;
        }

        if (this.ApplyPolicy(torrent, settings))
        {
            ditem.Status = DownloadItemStatus.Failed;
            ditem.Message = "The download has been stalled with no activity long enough to be considered failed";
            return;
        }

        ditem.Status = DownloadItemStatus.Warning;
        ditem.Message = "The download is stalled with no connections";
    }

    private bool ApplyPolicy(QBittorrentTorrent torrent, QBittorrentSettings settings)
        => settings.RemoveStalledDownloads
        && HasBeenInQueueLongEnough(torrent, settings)
        && HasNotBeenActive(torrent, settings);

    private bool HasNotBeenActive(QBittorrentTorrent torrent, QBittorrentSettings settings)
    {
        var lastActivity = DateTimeOffset.FromUnixTimeSeconds(torrent.LastActivity).DateTime;
        var threshold = TimeSpan.FromMinutes(settings.StalledInactivityThreshold);

        return _dateTime.Now - lastActivity > threshold;
    }

    private bool HasBeenInQueueLongEnough(QBittorrentTorrent torrent, QBittorrentSettings settings)
    {
        var addedOn = DateTimeOffset.FromUnixTimeSeconds(torrent.AddedOn).DateTime;
        var threshold = TimeSpan.FromMinutes(settings.StalledThreshold);

        return _dateTime.Now - addedOn > threshold;
    }
}
