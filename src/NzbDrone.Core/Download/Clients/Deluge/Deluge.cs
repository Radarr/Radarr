using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class Deluge : TorrentClientBase<DelugeSettings>
    {
        private readonly IDelugeProxy _proxy;

        public Deluge(IDelugeProxy proxy,
                      ITorrentFileInfoReader torrentFileInfoReader,
                      IHttpClient httpClient,
                      IConfigService configService,
                      IDiskProvider diskProvider,
                      IRemotePathMappingService remotePathMappingService,
                      ILocalizationService localizationService,
                      IBlocklistService blocklistService,
                      Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _proxy = proxy;
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            // set post-import category
            if (Settings.MovieImportedCategory.IsNotNullOrWhiteSpace() &&
                Settings.MovieImportedCategory != Settings.MovieCategory)
            {
                try
                {
                    _proxy.SetTorrentLabel(downloadClientItem.DownloadId.ToLower(), Settings.MovieImportedCategory, Settings);
                }
                catch (DownloadClientUnavailableException)
                {
                    _logger.Warn("Failed to set torrent post-import label \"{0}\" for {1} in Deluge. Does the label exist?",
                        Settings.MovieImportedCategory,
                        downloadClientItem.Title);
                }
            }
        }

        protected override string AddFromMagnetLink(RemoteMovie remoteMovie, string hash, string magnetLink)
        {
            var actualHash = _proxy.AddTorrentFromMagnet(magnetLink, Settings);

            if (actualHash.IsNullOrWhiteSpace())
            {
                throw new DownloadClientException("Deluge failed to add magnet " + magnetLink);
            }

            _proxy.SetTorrentSeedingConfiguration(actualHash, remoteMovie.SeedConfiguration, Settings);

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(actualHash, Settings.MovieCategory, Settings);
            }

            var isRecentMovie = remoteMovie.Movie.MovieMetadata.Value.IsRecentMovie;

            if ((isRecentMovie && Settings.RecentMoviePriority == (int)DelugePriority.First) ||
                (!isRecentMovie && Settings.OlderMoviePriority == (int)DelugePriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(actualHash, Settings);
            }

            return actualHash.ToUpper();
        }

        protected override string AddFromTorrentFile(RemoteMovie remoteMovie, string hash, string filename, byte[] fileContent)
        {
            var actualHash = _proxy.AddTorrentFromFile(filename, fileContent, Settings);

            if (actualHash.IsNullOrWhiteSpace())
            {
                throw new DownloadClientException("Deluge failed to add torrent " + filename);
            }

            _proxy.SetTorrentSeedingConfiguration(actualHash, remoteMovie.SeedConfiguration, Settings);

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(actualHash, Settings.MovieCategory, Settings);
            }

            var isRecentMovie = remoteMovie.Movie.MovieMetadata.Value.IsRecentMovie;

            if ((isRecentMovie && Settings.RecentMoviePriority == (int)DelugePriority.First) ||
                (!isRecentMovie && Settings.OlderMoviePriority == (int)DelugePriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(actualHash, Settings);
            }

            return actualHash.ToUpper();
        }

        public override string Name => "Deluge";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            IEnumerable<DelugeTorrent> torrents;

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                torrents = _proxy.GetTorrentsByLabel(Settings.MovieCategory, Settings);
            }
            else
            {
                torrents = _proxy.GetTorrents(Settings);
            }

            var items = new List<DownloadClientItem>();
            var ignoredCount = 0;

            foreach (var torrent in torrents)
            {
                // Silently ignore torrents with no hash
                if (torrent.Hash.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // Ignore torrents without a name, but track to log a single warning for all invalid torrents.
                if (torrent.Name.IsNullOrWhiteSpace())
                {
                    ignoredCount++;
                    continue;
                }

                var item = new DownloadClientItem();
                item.DownloadId = torrent.Hash.ToUpper();
                item.Title = torrent.Name;
                item.Category = Settings.MovieCategory;

                item.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, Settings.MovieImportedCategory.IsNotNullOrWhiteSpace());

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.DownloadPath));
                item.OutputPath = outputPath + torrent.Name;
                item.RemainingSize = torrent.Size - torrent.BytesDownloaded;
                item.SeedRatio = torrent.Ratio;

                try
                {
                    item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                }
                catch (OverflowException ex)
                {
                    _logger.Debug(ex, "ETA for {0} is too long: {1}", torrent.Name, torrent.Eta);
                    item.RemainingTime = TimeSpan.MaxValue;
                }

                item.TotalSize = torrent.Size;

                if (torrent.State == DelugeTorrentStatus.Error)
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = _localizationService.GetLocalizedString("DownloadClientDelugeTorrentStateError");
                }
                else if (torrent.IsFinished && torrent.State != DelugeTorrentStatus.Checking)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.State == DelugeTorrentStatus.Queued)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else if (torrent.State == DelugeTorrentStatus.Paused)
                {
                    item.Status = DownloadItemStatus.Paused;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                // Here we detect if Deluge is managing the torrent and whether the seed criteria has been met.
                // This allows Radarr to delete the torrent as appropriate.
                item.CanMoveFiles = item.CanBeRemoved =
                    item.DownloadClientInfo.RemoveCompletedDownloads &&
                    torrent.IsAutoManaged &&
                    torrent.StopAtRatio &&
                    torrent.Ratio >= torrent.StopRatio &&
                    torrent.State == DelugeTorrentStatus.Paused;

                items.Add(item);
            }

            if (ignoredCount > 0)
            {
                _logger.Warn("{0} torrent(s) were ignored because they did not have a title, check Deluge and remove any invalid torrents");
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            _proxy.RemoveTorrent(item.DownloadId.ToLower(), deleteData, Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var label = _proxy.GetLabelOptions(Settings);
            OsPath destDir;

            if (label != null && label.ApplyMoveCompleted && label.MoveCompleted)
            {
                // if label exists and a label completed path exists and is enabled use it instead of global
                destDir = new OsPath(label.MoveCompletedPath);
            }
            else if (config.GetValueOrDefault("move_completed", false).ToString() == "True")
            {
                destDir = new OsPath(config.GetValueOrDefault("move_completed_path") as string);
            }
            else
            {
                destDir = new OsPath(config.GetValueOrDefault("download_location") as string);
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

            failures.AddIfNotNull(TestCategory());
            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetVersion(Settings);
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Password", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"));
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to test connection");
                switch (ex.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                        return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnectDetail")
                        };
                    case WebExceptionStatus.ConnectionClosed:
                        return new NzbDroneValidationFailure("UseSsl", _localizationService.GetLocalizedString("DownloadClientValidationVerifySsl"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationVerifySslDetail", new Dictionary<string, object> { { "clientName", Name } })
                        };
                    case WebExceptionStatus.SecureChannelFailure:
                        return new NzbDroneValidationFailure("UseSsl", _localizationService.GetLocalizedString("DownloadClientValidationSslConnectFailure"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationSslConnectFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                        };
                    default:
                        return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test connection");

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                       {
                           DetailedDescription = ex.Message
                       };
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            if (Settings.MovieCategory.IsNullOrWhiteSpace() && Settings.MovieImportedCategory.IsNullOrWhiteSpace())
            {
                return null;
            }

            var enabledPlugins = _proxy.GetEnabledPlugins(Settings);

            if (!enabledPlugins.Contains("Label"))
            {
                return new NzbDroneValidationFailure("MovieCategory", _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginInactive"))
                {
                    DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginInactiveDetail", new Dictionary<string, object> { { "clientName", Name } })
                };
            }

            var labels = _proxy.GetAvailableLabels(Settings);

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace() && !labels.Contains(Settings.MovieCategory))
            {
                _proxy.AddLabel(Settings.MovieCategory, Settings);
                labels = _proxy.GetAvailableLabels(Settings);

                if (!labels.Contains(Settings.MovieCategory))
                {
                    return new NzbDroneValidationFailure("MovieCategory", _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginFailure"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                    };
                }
            }

            if (Settings.MovieImportedCategory.IsNotNullOrWhiteSpace() && !labels.Contains(Settings.MovieImportedCategory))
            {
                _proxy.AddLabel(Settings.MovieImportedCategory, Settings);
                labels = _proxy.GetAvailableLabels(Settings);

                if (!labels.Contains(Settings.MovieImportedCategory))
                {
                    return new NzbDroneValidationFailure("MovieImportedCategory", _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginFailure"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDelugeValidationLabelPluginFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                    };
                }
            }

            return null;
        }

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                _proxy.GetTorrents(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to get torrents");
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestTorrents", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
