using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrent : TorrentClientBase<UTorrentSettings>
    {
        private readonly IUTorrentProxy _proxy;
        private readonly ICached<UTorrentTorrentCache> _torrentCache;

        public UTorrent(IUTorrentProxy proxy,
                        ICacheManager cacheManager,
                        ITorrentFileInfoReader torrentFileInfoReader,
                        IHttpClient httpClient,
                        IConfigService configService,
                        INamingConfigService namingConfigService,
                        IDiskProvider diskProvider,
                        IRemotePathMappingService remotePathMappingService,
                        Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, namingConfigService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;

            _torrentCache = cacheManager.GetCache<UTorrentTorrentCache>(GetType(), "differentialTorrents");
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            // set post-import category
            if (Settings.MovieImportedCategory.IsNotNullOrWhiteSpace() &&
                Settings.MovieImportedCategory != Settings.MovieCategory)
            {
                _proxy.SetTorrentLabel(downloadClientItem.DownloadId.ToLower(), Settings.MovieImportedCategory, Settings);

                // old label must be explicitly removed
                if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
                {
                    _proxy.RemoveTorrentLabel(downloadClientItem.DownloadId.ToLower(), Settings.MovieCategory, Settings);
                }
            }
        }

        protected override string AddFromMagnetLink(RemoteMovie remoteMovie, string hash, string magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, Settings);
            _proxy.SetTorrentSeedingConfiguration(hash, remoteMovie.SeedConfiguration, Settings);

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(hash, Settings.MovieCategory, Settings);
            }

            var isRecentMovie = remoteMovie.Movie.IsRecentMovie;

            if ((isRecentMovie && Settings.RecentMoviePriority == (int)UTorrentPriority.First) ||
                (!isRecentMovie && Settings.OlderMoviePriority == (int)UTorrentPriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            _proxy.SetState(hash, (UTorrentState)Settings.IntialState, Settings);

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteMovie remoteMovie, string hash, string filename, byte[] fileContent)
        {
            _proxy.AddTorrentFromFile(filename, fileContent, Settings);
            _proxy.SetTorrentSeedingConfiguration(hash, remoteMovie.SeedConfiguration, Settings);

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(hash, Settings.MovieCategory, Settings);
            }

            var isRecentEpisode = remoteMovie.Movie.IsRecentMovie;

            if ((isRecentEpisode && Settings.RecentMoviePriority == (int)UTorrentPriority.First) ||
                (!isRecentEpisode && Settings.OlderMoviePriority == (int)UTorrentPriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            _proxy.SetState(hash, (UTorrentState)Settings.IntialState, Settings);

            return hash;
        }

        public override string Name => "uTorrent";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var torrents = GetTorrents();

            var queueItems = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                if (torrent.Label != Settings.MovieCategory)
                {
                    continue;
                }

                var item = new DownloadClientItem();
                item.DownloadId = torrent.Hash;
                item.Title = torrent.Name;
                item.TotalSize = torrent.Size;
                item.Category = torrent.Label;
                item.DownloadClient = Definition.Name;
                item.RemainingSize = torrent.Remaining;
                item.SeedRatio = torrent.Ratio;

                if (torrent.Eta != -1)
                {
                    item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                }

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.RootDownloadPath));

                if (outputPath.FileName == torrent.Name)
                {
                    item.OutputPath = outputPath;
                }
                else
                {
                    item.OutputPath = outputPath + torrent.Name;
                }

                if (torrent.Status.HasFlag(UTorrentTorrentStatus.Error))
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = "uTorrent is reporting an error";
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Loaded) &&
                         torrent.Status.HasFlag(UTorrentTorrentStatus.Checked) && torrent.Remaining == 0 && torrent.Progress == 1.0)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Paused))
                {
                    item.Status = DownloadItemStatus.Paused;
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Started))
                {
                    item.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    item.Status = DownloadItemStatus.Queued;
                }

                // 'Started' without 'Queued' is when the torrent is 'forced seeding'
                item.CanMoveFiles = item.CanBeRemoved =
                    !torrent.Status.HasFlag(UTorrentTorrentStatus.Queued) &&
                    !torrent.Status.HasFlag(UTorrentTorrentStatus.Started);

                queueItems.Add(item);
            }

            return queueItems;
        }

        private List<UTorrentTorrent> GetTorrents()
        {
            List<UTorrentTorrent> torrents;

            var cacheKey = string.Format("{0}:{1}:{2}", Settings.Host, Settings.Port, Settings.MovieCategory);
            var cache = _torrentCache.Find(cacheKey);

            var response = _proxy.GetTorrents(cache == null ? null : cache.CacheID, Settings);

            if (cache != null && response.Torrents == null)
            {
                var removedAndUpdated = new HashSet<string>(response.TorrentsChanged.Select(v => v.Hash).Concat(response.TorrentsRemoved));

                torrents = cache.Torrents
                    .Where(v => !removedAndUpdated.Contains(v.Hash))
                    .Concat(response.TorrentsChanged)
                    .ToList();
            }
            else
            {
                torrents = response.Torrents;
            }

            cache = new UTorrentTorrentCache
            {
                CacheID = response.CacheNumber,
                Torrents = torrents
            };

            _torrentCache.Set(cacheKey, cache, TimeSpan.FromMinutes(15));

            return torrents;
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveTorrent(downloadId, deleteData, Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            var config = _proxy.GetConfig(Settings);

            OsPath destDir = new OsPath(null);

            if (config.GetValueOrDefault("dir_active_download_flag") == "true")
            {
                destDir = new OsPath(config.GetValueOrDefault("dir_active_download"));
            }

            if (config.GetValueOrDefault("dir_completed_download_flag") == "true")
            {
                destDir = new OsPath(config.GetValueOrDefault("dir_completed_download"));

                if (config.GetValueOrDefault("dir_add_label") == "true")
                {
                    destDir = destDir + Settings.MovieCategory;
                }
            }

            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            if (!destDir.IsEmpty)
            {
                status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) };
            }

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxy.GetVersion(Settings);

                if (version < 25406)
                {
                    return new ValidationFailure(string.Empty, "Old uTorrent client with unsupported API, need 3.0 or higher");
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = "Please verify your username and password."
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to connect to uTorrent");
                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new NzbDroneValidationFailure("Host", "Unable to connect")
                    {
                        DetailedDescription = "Please verify the hostname and port."
                    };
                }

                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test uTorrent");
                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }

            return null;
        }

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                _proxy.GetTorrents(null, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get torrents");
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
