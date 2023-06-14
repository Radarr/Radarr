using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public abstract class TransmissionBase : TorrentClientBase<TransmissionSettings>
    {
        protected readonly ITransmissionProxy _proxy;

        public TransmissionBase(ITransmissionProxy proxy,
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
        }

        private static IEnumerable<string> HandleTags(RemoteMovie remoteMovie, TransmissionSettings settings)
        {
            var result = new HashSet<string>();

            if (settings.Labels.Any())
            {
                result.UnionWith(settings.Labels);
            }

            if (settings.AdditionalLabels.Any())
            {
                foreach (var additionalTag in settings.AdditionalLabels)
                {
                    switch (additionalTag)
                    {
                        case (int)AdditionalLabels.Collection:
                            result.Add(remoteMovie.Movie.MovieMetadata.Value.CollectionTitle);
                            break;
                        case (int)AdditionalLabels.Quality:
                            result.Add(remoteMovie.ParsedMovieInfo.Quality.Quality.ToString());
                            break;
                        case (int)AdditionalLabels.Languages:
                            result.UnionWith(remoteMovie.Languages.ConvertAll(language => language.ToString()));
                            break;
                        case (int)AdditionalLabels.ReleaseGroup:
                            result.Add(remoteMovie.ParsedMovieInfo.ReleaseGroup);
                            break;
                        case (int)AdditionalLabels.Year:
                            result.Add(remoteMovie.Movie.Year.ToString());
                            break;
                        case (int)AdditionalLabels.Indexer:
                            result.Add(remoteMovie.Release.Indexer);
                            break;
                        case (int)AdditionalLabels.Studio:
                            result.Add(remoteMovie.Movie.MovieMetadata.Value.Studio);
                            break;
                        default:
                            throw new DownloadClientException("Unexpected additional tag ID");
                    }
                }
            }

            return result;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var configFunc = new Lazy<TransmissionConfig>(() => _proxy.GetConfig(Settings));
            var torrents = _proxy.GetTorrents(Settings);

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                // If totalsize == 0 the torrent is a magnet downloading metadata
                if (torrent.TotalSize == 0)
                {
                    continue;
                }

                var outputPath = new OsPath(torrent.DownloadDir);

                if (Settings.MovieDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath(Settings.MovieDirectory).Contains(outputPath))
                    {
                        continue;
                    }
                }
                else if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.MovieCategory))
                    {
                        continue;
                    }
                }

                outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, outputPath);

                if (_proxy.SupportLabels(Settings) && !Settings.Labels.All(label => torrent.Labels.Contains(label)))
                {
                    continue;
                }

                var item = new DownloadClientItem();
                item.DownloadId = torrent.HashString.ToUpper();

                if (!_proxy.SupportLabels(Settings))
                {
                    item.Category = Settings.MovieCategory;
                }
                else
                {
                    item.Category = torrent.Labels.FirstOrDefault();
                }

                item.Title = torrent.Name;

                item.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this);

                item.OutputPath = GetOutputPath(outputPath, torrent);
                item.TotalSize = torrent.TotalSize;
                item.RemainingSize = torrent.LeftUntilDone;
                item.SeedRatio = torrent.DownloadedEver <= 0 ? 0 :
                    (double)torrent.UploadedEver / torrent.DownloadedEver;

                if (torrent.Eta >= 0)
                {
                    try
                    {
                        item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                    }
                    catch (OverflowException)
                    {
                        item.RemainingTime = TimeSpan.FromMilliseconds(torrent.Eta);
                    }
                }

                if (!torrent.ErrorString.IsNullOrWhiteSpace())
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.ErrorString;
                }
                else if (torrent.LeftUntilDone == 0 && (torrent.Status == TransmissionTorrentStatus.Stopped ||
                                                        torrent.Status == TransmissionTorrentStatus.Seeding ||
                                                        torrent.Status == TransmissionTorrentStatus.SeedingWait))
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.IsFinished && torrent.Status != TransmissionTorrentStatus.Check &&
                         torrent.Status != TransmissionTorrentStatus.CheckWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status == TransmissionTorrentStatus.Queued)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                item.CanBeRemoved = HasReachedSeedLimit(torrent, item.SeedRatio, configFunc);
                item.CanMoveFiles = item.CanBeRemoved && torrent.Status == TransmissionTorrentStatus.Stopped;

                items.Add(item);
            }

            return items;
        }

        protected bool HasReachedSeedLimit(TransmissionTorrent torrent, double? ratio, Lazy<TransmissionConfig> config)
        {
            var isStopped = torrent.Status == TransmissionTorrentStatus.Stopped;
            var isSeeding = torrent.Status == TransmissionTorrentStatus.Seeding;

            if (torrent.SeedRatioMode == 1)
            {
                if (isStopped && ratio.HasValue && ratio >= torrent.SeedRatioLimit)
                {
                    return true;
                }
            }
            else if (torrent.SeedRatioMode == 0)
            {
                if (isStopped && config.Value.SeedRatioLimited && ratio >= config.Value.SeedRatioLimit)
                {
                    return true;
                }
            }

            // Transmission doesn't support SeedTimeLimit, use/abuse seed idle limit, but only if it was set per-torrent.
            if (torrent.SeedIdleMode == 1)
            {
                if ((isStopped || isSeeding) && torrent.SecondsSeeding > torrent.SeedIdleLimit * 60)
                {
                    return true;
                }
            }
            else if (torrent.SeedIdleMode == 0)
            {
                // The global idle limit is a real idle limit, if it's configured then 'Stopped' is enough.
                if (isStopped && config.Value.IdleSeedingLimitEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            _proxy.RemoveTorrent(item.DownloadId.ToLower(), deleteData, Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            string destDir;
            if (Settings.MovieDirectory.IsNotNullOrWhiteSpace())
            {
                destDir = Settings.MovieDirectory;
            }
            else
            {
                var config = _proxy.GetConfig(Settings);
                destDir = config.DownloadDir;

                if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
                {
                    destDir = string.Format("{0}/{1}", destDir, Settings.MovieCategory);
                }
            }

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir)) }
            };
        }

        protected override string AddFromMagnetLink(RemoteMovie remoteMovie, string hash, string magnetLink)
        {
            var labels = _proxy.SupportLabels(Settings) ? HandleTags(remoteMovie, Settings) : null;

            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(), labels, Settings);
            _proxy.SetTorrentSeedingConfiguration(hash, remoteMovie.SeedConfiguration, Settings);

            var isRecentMovie = remoteMovie.Movie.MovieMetadata.Value.IsRecentMovie;

            if ((isRecentMovie && Settings.RecentMoviePriority == (int)TransmissionPriority.First) ||
                (!isRecentMovie && Settings.OlderMoviePriority == (int)TransmissionPriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteMovie remoteMovie, string hash, string filename, byte[] fileContent)
        {
            var labels = _proxy.SupportLabels(Settings) ? HandleTags(remoteMovie, Settings) : null;

            _proxy.AddTorrentFromData(fileContent, GetDownloadDirectory(), labels, Settings);
            _proxy.SetTorrentSeedingConfiguration(hash, remoteMovie.SeedConfiguration, Settings);

            var isRecentMovie = remoteMovie.Movie.MovieMetadata.Value.IsRecentMovie;

            if ((isRecentMovie && Settings.RecentMoviePriority == (int)TransmissionPriority.First) ||
                (!isRecentMovie && Settings.OlderMoviePriority == (int)TransmissionPriority.First))
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            if (_proxy.SupportLabels(Settings) && Settings.PostImportLabels.Any())
            {
                try
                {
                    var torrent = _proxy.GetSingleTorrent(downloadClientItem.DownloadId.ToLower(), Settings);
                    var labels = torrent.Labels.Concat(Settings.PostImportLabels).Distinct().ToList();

                    if (Settings.Labels.Any())
                    {
                        foreach (var label in Settings.Labels)
                        {
                            labels.Remove(label);
                        }
                    }

                    _proxy.SetTorrentLabel(downloadClientItem.DownloadId.ToLower(), labels, Settings);
                }
                catch (DownloadClientException)
                {
                    _logger.Warn("Failed to add post-import torrent label \"{0}\" for {1} in Transmission. Does the label exist?", Settings.PostImportLabels, downloadClientItem.Title);
                }
            }
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

        protected virtual OsPath GetOutputPath(OsPath outputPath, TransmissionTorrent torrent)
        {
            return outputPath + torrent.Name.Replace(":", "_");
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.MovieDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.MovieDirectory;
            }

            if (!Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                return null;
            }

            var config = _proxy.GetConfig(Settings);
            var destDir = config.DownloadDir;

            return $"{destDir.TrimEnd('/')}/{Settings.MovieCategory}";
        }

        protected ValidationFailure TestConnection()
        {
            try
            {
                return ValidateVersion();
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = string.Format("Please verify your username and password. Also verify if the host running Radarr isn't blocked from accessing {0} by WhiteList limitations in the {0} configuration.", Name)
                };
            }
            catch (DownloadClientUnavailableException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Host", "Unable to connect to Transmission")
                {
                    DetailedDescription = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test");

                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
        }

        protected abstract ValidationFailure ValidateVersion();

        private ValidationFailure TestGetTorrents()
        {
            _logger.Trace("Fra: " + Settings.Host);
            try
            {
                _proxy.GetTorrents(Settings);
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
