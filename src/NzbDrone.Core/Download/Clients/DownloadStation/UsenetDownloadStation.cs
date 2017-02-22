﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class UsenetDownloadStation : UsenetClientBase<DownloadStationSettings>
    {
        protected readonly IDownloadStationProxy _proxy;
        protected readonly ISharedFolderResolver _sharedFolderResolver;
        protected readonly ISerialNumberProvider _serialNumberProvider;
        protected readonly IFileStationProxy _fileStationProxy;

        public UsenetDownloadStation(ISharedFolderResolver sharedFolderResolver,
                                     ISerialNumberProvider serialNumberProvider,
                                     IFileStationProxy fileStationProxy,
                                     IDownloadStationProxy proxy,
                                     IHttpClient httpClient,
                                     IConfigService configService,
                                     IDiskProvider diskProvider,
                                     IRemotePathMappingService remotePathMappingService,
                                     Logger logger
                                     )
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
            _fileStationProxy = fileStationProxy;
            _sharedFolderResolver = sharedFolderResolver;
            _serialNumberProvider = serialNumberProvider;
        }

        public override string Name => "Download Station";

        protected IEnumerable<DownloadStationTask> GetTasks()
        {
            return _proxy.GetTasks(Settings).Where(v => v.Type == DownloadStationTaskType.NZB.ToString());
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var nzbTasks = GetTasks();
            var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            var items = new List<DownloadClientItem>();

            long totalRemainingSize = 0;
            long globalSpeed = nzbTasks.Where(t => t.Status == DownloadStationTaskStatus.Downloading)
                                       .Select(GetDownloadSpeed)
                                       .Sum();

            foreach (var nzb in nzbTasks)
            {
                var outputPath = new OsPath($"/{nzb.Additional.Detail["destination"]}");

                var taskRemainingSize = GetRemainingSize(nzb);

                if (nzb.Status != DownloadStationTaskStatus.Paused)
                {
                    totalRemainingSize += taskRemainingSize;
                }

                if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath($"/{Settings.TvDirectory}").Contains(outputPath))
                    {
                        continue;
                    }
                }
                else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.TvCategory))
                    {
                        continue;
                    }
                }

                var item = new DownloadClientItem()
                {
                    Category = Settings.TvCategory,
                    DownloadClient = Definition.Name,
                    DownloadId = CreateDownloadId(nzb.Id, serialNumber),
                    Title = nzb.Title,
                    TotalSize = nzb.Size,
                    RemainingSize = taskRemainingSize,
                    Status = GetStatus(nzb),
                    Message = GetMessage(nzb),
                    IsReadOnly = !IsFinished(nzb)
                };

                if (item.Status != DownloadItemStatus.Paused)
                {
                    item.RemainingTime = GetRemainingTime(totalRemainingSize, globalSpeed);
                }

                if (item.Status == DownloadItemStatus.Completed || item.Status == DownloadItemStatus.Failed)
                {
                    item.OutputPath = GetOutputPath(outputPath, nzb, serialNumber);
                }

                items.Add(item);
            }

            return items;
        }

        protected OsPath GetOutputPath(OsPath outputPath, DownloadStationTask task, string serialNumber)
        {
            var fullPath = _sharedFolderResolver.RemapToFullPath(outputPath, Settings, serialNumber);

            var remotePath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath);

            var finalPath = remotePath + task.Title;

            return finalPath;
        }

        public override DownloadClientStatus GetStatus()
        {
            try
            {
                var path = GetDownloadDirectory();

                return new DownloadClientStatus
                {
                    IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                    OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(path)) }
                };
            }
            catch (DownloadClientException e)
            {
                _logger.Debug(e, "Failed to get config from Download Station");

                throw e;
            }
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(downloadId);
            }

            _proxy.RemoveTask(ParseDownloadId(downloadId), Settings);
            _logger.Debug("{0} removed correctly", downloadId);
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            throw new DownloadClientException("Episodes are not working with Radarr");
        }

        protected override string AddFromNzbFile(RemoteMovie remoteEpisode, string filename, byte[] fileContent)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            _proxy.AddTaskFromData(fileContent, filename, GetDownloadDirectory(), Settings);

            var items = GetTasks().Where(t => t.Additional.Detail["uri"] == filename);

            var item = items.SingleOrDefault();

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteEpisode);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", filename);

            throw new DownloadClientException("Failed to add NZB task to Download Station");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestOutputPath());
            failures.AddIfNotNull(TestGetNZB());
        }

        protected ValidationFailure TestOutputPath()
        {
            try
            {
                var downloadDir = GetDownloadDirectory();

                if (downloadDir != null)
                {
                    var sharedFolder = downloadDir.Split('\\', '/')[0];
                    var fieldName = Settings.TvDirectory.IsNotNullOrWhiteSpace() ? nameof(Settings.TvDirectory) : nameof(Settings.TvCategory);

                    var folderInfo = _fileStationProxy.GetInfoFileOrDirectory($"/{downloadDir}", Settings);

                    if (folderInfo.Additional == null)
                    {
                        return new NzbDroneValidationFailure(fieldName, $"Shared folder does not exist")
                        {
                            DetailedDescription = $"The DownloadStation does not have a Shared Folder with the name '{sharedFolder}', are you sure you specified it correctly?"
                        };
                    }

                    if (!folderInfo.IsDir)
                    {
                        return new NzbDroneValidationFailure(fieldName, $"Folder does not exist")
                        {
                            DetailedDescription = $"The folder '{downloadDir}' does not exist, it must be created manually inside the Shared Folder '{sharedFolder}'."
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new NzbDroneValidationFailure(string.Empty, $"Unknown exception: {ex.Message}");
            }
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
                    DetailedDescription = $"Please verify your username and password. Also verify if the host running Sonarr isn't blocked from accessing {Name} by WhiteList limitations in the {Name} configuration."
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex);

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
                _logger.Error(ex);
                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
        }

        protected ValidationFailure ValidateVersion()
        {
            var versionRange = _proxy.GetApiVersion(Settings);

            _logger.Debug("Download Station api version information: Min {0} - Max {1}", versionRange.Min(), versionRange.Max());

            if (!versionRange.Contains(2))
            {
                return new ValidationFailure(string.Empty, $"Download Station API version not supported, should be at least 2. It supports from {versionRange.Min()} to {versionRange.Max()}");
            }

            return null;
        }

        protected bool IsFinished(DownloadStationTask task)
        {
            return task.Status == DownloadStationTaskStatus.Finished;
        }

        protected string GetMessage(DownloadStationTask task)
        {
            if (task.StatusExtra != null)
            {
                if (task.Status == DownloadStationTaskStatus.Extracting)
                {
                    return $"Extracting: {int.Parse(task.StatusExtra["unzip_progress"])}%";
                }

                if (task.Status == DownloadStationTaskStatus.Error)
                {
                    return task.StatusExtra["error_detail"];
                }
            }

            return null;
        }

        protected DownloadItemStatus GetStatus(DownloadStationTask task)
        {
            switch (task.Status)
            {
                case DownloadStationTaskStatus.Waiting:
                    return task.Size == 0 || GetRemainingSize(task) > 0 ? DownloadItemStatus.Queued : DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Paused:
                    return DownloadItemStatus.Paused;
                case DownloadStationTaskStatus.Finished:
                case DownloadStationTaskStatus.Seeding:
                    return DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Error:
                    return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }

        protected long GetRemainingSize(DownloadStationTask task)
        {
            var downloadedString = task.Additional.Transfer["size_downloaded"];
            long downloadedSize;

            if (downloadedString.IsNullOrWhiteSpace() || !long.TryParse(downloadedString, out downloadedSize))
            {
                _logger.Debug("Task {0} has invalid size_downloaded: {1}", task.Title, downloadedString);
                downloadedSize = 0;
            }

            return task.Size - Math.Max(0, downloadedSize);
        }

        protected long GetDownloadSpeed(DownloadStationTask task)
        {
            var speedString = task.Additional.Transfer["speed_download"];
            long downloadSpeed;

            if (speedString.IsNullOrWhiteSpace() || !long.TryParse(speedString, out downloadSpeed))
            {
                _logger.Debug("Task {0} has invalid speed_download: {1}", task.Title, speedString);
                downloadSpeed = 0;
            }

            return Math.Max(downloadSpeed, 0);
        }

        protected TimeSpan? GetRemainingTime(long remainingSize, long downloadSpeed)
        {
            if (downloadSpeed > 0)
            {
                return TimeSpan.FromSeconds(remainingSize / downloadSpeed);
            }
            else
            {
                return null;
            }
        }

        protected ValidationFailure TestGetNZB()
        {
            try
            {
                GetItems();
                return null;
            }
            catch (Exception ex)
            {
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of NZBs: " + ex.Message);
            }
        }

        protected string ParseDownloadId(string id)
        {
            return id.Split(':')[1];
        }

        protected string CreateDownloadId(string id, string hashedSerialNumber)
        {
            return $"{hashedSerialNumber}:{id}";
        }

        protected string GetDefaultDir()
        {
            var config = _proxy.GetConfig(Settings);

            var path = config["default_destination"] as string;

            return path;
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.TvDirectory.TrimStart('/');
            }
            else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                var destDir = GetDefaultDir();

                return $"{destDir.TrimEnd('/')}/{Settings.TvCategory}";
            }

            return null;
        }
    }
}
