using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.QBittorrent;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.QBittorrentTests;

[TestFixture]
public sealed class StalledTorrentPolicyFixture
{
    private static readonly DateTime _defaultDateTime = new DateTime(year: 1991, month: 8, day: 25).ToUniversalTime();
    private static readonly IDateTimeProvider _defaultProvider = new StubDateTimeProvider { Now = _defaultDateTime };

    private static long ToUnixTimeStamp(DateTime datetime) => ((DateTimeOffset)datetime).ToUnixTimeSeconds();

    [Test]
    public void ShouldNotIntroduceSideEffectIfNotAStaleDownload()
    {
        var config = new QBittorrentSettings { RemoveStalledDownloads = true, };
        var policy = new QBittorrentStalledPolicy(_defaultProvider);
        var torrent = new QBittorrentTorrent { };
        var item = new DownloadClientItem { Status = DownloadItemStatus.Downloading, Message = "placeholder" };

        policy.Apply(torrent, item, config);

        item.Status.Should().Be(DownloadItemStatus.Downloading);
        item.Message.Should().Be("placeholder");
    }

    [Test]
    public void ShouldDefaultIfStaleHandlingIsDisabledEvenIfAllOtherConditionsAreMet()
    {
        var queuedThresholdMins = 10;
        var inactivityThresholdMins = 1;
        var config = new QBittorrentSettings { RemoveStalledDownloads = false, StalledThreshold = queuedThresholdMins, StalledInactivityThreshold = inactivityThresholdMins };
        var policy = new QBittorrentStalledPolicy(_defaultProvider);
        var addedOn = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(queuedThresholdMins) - TimeSpan.FromSeconds(1));
        var lastActivity = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(inactivityThresholdMins) - TimeSpan.FromSeconds(1));
        var torrent = new QBittorrentTorrent { State = "stalledDL", AddedOn = addedOn, LastActivity = lastActivity };
        var item = new DownloadClientItem { Status = DownloadItemStatus.Downloading, Message = "placeholder" };

        policy.Apply(torrent, item, config);

        item.Status.Should().Be(DownloadItemStatus.Warning);
        item.Message.Should().Be("The download is stalled with no connections");
    }

    [Test]
    public void ShouldNotMarkFailedIfNotQueuedLongEnoughEvenIfInactiveBeyondThreshold()
    {
        var queuedThresholdMins = 10;
        var inactivityThresholdMins = 1;
        var config = new QBittorrentSettings { RemoveStalledDownloads = true, StalledThreshold = queuedThresholdMins, StalledInactivityThreshold = inactivityThresholdMins };
        var policy = new QBittorrentStalledPolicy(_defaultProvider);
        var addedOn = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(queuedThresholdMins));
        var lastActivity = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(queuedThresholdMins) - TimeSpan.FromSeconds(1));
        var torrent = new QBittorrentTorrent { State = "stalledDL", AddedOn = addedOn, LastActivity = lastActivity };
        var item = new DownloadClientItem { Status = DownloadItemStatus.Downloading, Message = "placeholder" };

        policy.Apply(torrent, item, config);

        item.Status.Should().Be(DownloadItemStatus.Warning);
        item.Message.Should().Be("The download is stalled with no connections");
    }

    [Test]
    public void ShouldNotMarkFailedIfQueuedLongEnoughButActiveRecentlyEnough()
    {
        var queuedThresholdMins = 10;
        var inactivityThresholdMins = 5;
        var config = new QBittorrentSettings { RemoveStalledDownloads = true, StalledThreshold = queuedThresholdMins, StalledInactivityThreshold = inactivityThresholdMins };
        var policy = new QBittorrentStalledPolicy(_defaultProvider);
        var addedOn = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(queuedThresholdMins) - TimeSpan.FromSeconds(1));
        var lastActivity = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(inactivityThresholdMins));
        var torrent = new QBittorrentTorrent { State = "stalledDL", AddedOn = addedOn, LastActivity = lastActivity };
        var item = new DownloadClientItem { Status = DownloadItemStatus.Downloading, Message = "placeholder" };

        policy.Apply(torrent, item, config);

        item.Status.Should().Be(DownloadItemStatus.Warning);
        item.Message.Should().Be("The download is stalled with no connections");
    }

    [Test]
    public void ShouldMarkFailedIfQueuedLongEnoughAndNotActiveRecentlyEnough()
    {
        var queuedThresholdMins = 10;
        var inactivityThresholdMins = 5;
        var config = new QBittorrentSettings { RemoveStalledDownloads = true, StalledThreshold = queuedThresholdMins, StalledInactivityThreshold = inactivityThresholdMins };
        var policy = new QBittorrentStalledPolicy(_defaultProvider);
        var addedOn = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(queuedThresholdMins) - TimeSpan.FromSeconds(1));
        var lastActivity = ToUnixTimeStamp(_defaultDateTime - TimeSpan.FromMinutes(inactivityThresholdMins) - TimeSpan.FromSeconds(1));
        var torrent = new QBittorrentTorrent { State = "stalledDL", AddedOn = addedOn, LastActivity = lastActivity };
        var item = new DownloadClientItem { Status = DownloadItemStatus.Downloading, Message = "placeholder" };

        policy.Apply(torrent, item, config);

        item.Status.Should().Be(DownloadItemStatus.Failed);
        item.Message.Should().Be("The download has been stalled with no activity long enough to be considered failed");
    }

    private sealed class StubDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now { get; set; }
    }
}
