using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderAddedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]
    [CheckOn(typeof(MovieImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(MovieImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RemotePathMappingCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly IOsInfo _osInfo;

        public RemotePathMappingCheck(IDiskProvider diskProvider,
                                      IProvideDownloadClient downloadClientProvider,
                                      IConfigService configService,
                                      IOsInfo osInfo,
                                      Logger logger,
                                      ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _downloadClientProvider = downloadClientProvider;
            _configService = configService;
            _logger = logger;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            // We don't care about client folders if we are not handling completed files
            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType());
            }

            var clients = _downloadClientProvider.GetDownloadClients();

            foreach (var client in clients)
            {
                try
                {
                    var status = client.GetStatus();
                    var folders = status.OutputRootFolders;
                    foreach (var folder in folders)
                    {
                        if (!folder.IsValid)
                        {
                            if (!status.IsLocalhost)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckWrongOSPath"), client.Definition.Name, folder.FullPath, _osInfo.Name), "#bad_remote_path_mapping");
                            }
                            else if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckBadDockerPath"), client.Definition.Name, folder.FullPath, _osInfo.Name), "#docker_bad_remote_path_mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckLocalWrongOSPath"), client.Definition.Name, folder.FullPath, _osInfo.Name), "#bad_download_client_settings");
                            }
                        }

                        if (!_diskProvider.FolderExists(folder.FullPath))
                        {
                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckDockerFolderMissing"), client.Definition.Name, folder.FullPath), "#docker_bad_remote_path_mapping");
                            }
                            else if (!status.IsLocalhost)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckLocalFolderMissing"), client.Definition.Name, folder.FullPath), "#bad_remote_path_mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckGenericPermissions"), client.Definition.Name, folder.FullPath), "#permissions_error");
                            }
                        }
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in RemotePathMapping HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }

        public HealthCheck Check(IEvent message)
        {
            // We don't care about client folders if we are not handling completed files
            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType());
            }

            if (typeof(MovieImportFailedEvent).IsAssignableFrom(message.GetType()))
            {
                var failureMessage = (MovieImportFailedEvent)message;

                // if we can see the file exists but the import failed then likely a permissions issue
                if (failureMessage.MovieInfo != null)
                {
                    var moviePath = failureMessage.MovieInfo.Path;
                    if (_diskProvider.FileExists(moviePath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckDownloadPermissions"), moviePath), "#permissions_error");
                    }
                    else
                    {
                        // If the file doesn't exist but MovieInfo is not null then the message is coming from
                        // ImportApprovedMovies and the file must have been removed part way through processing
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFileRemoved"), moviePath), "#remote_path_file_removed");
                    }
                }

                // If the previous case did not match then the failure occured in DownloadedMovieImportService,
                // while trying to locate the files reported by the download client
                var client = _downloadClientProvider.GetDownloadClients().FirstOrDefault(x => x.Definition.Name == failureMessage.DownloadClientInfo.Name);
                try
                {
                    var status = client.GetStatus();
                    var dlpath = client?.GetItems().FirstOrDefault(x => x.DownloadId == failureMessage.DownloadId)?.OutputPath.FullPath;

                    // If dlpath is null then there's not much useful we can report.  Give a generic message so
                    // that the user realises something is wrong.
                    if (dlpath.IsNullOrWhiteSpace())
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("RemotePathMappingCheckImportFailed"), "#remote_path_import_failed");
                    }

                    if (!dlpath.IsPathValid())
                    {
                        if (!status.IsLocalhost)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFilesWrongOSPath"), client.Definition.Name, dlpath, _osInfo.Name), "#bad_remote_path_mapping");
                        }
                        else if (_osInfo.IsDocker)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFilesBadDockerPath"), client.Definition.Name, dlpath, _osInfo.Name), "#docker_bad_remote_path_mapping");
                        }
                        else
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFilesLocalWrongOSPath"), client.Definition.Name, dlpath, _osInfo.Name), "#bad_download_client_settings");
                        }
                    }

                    if (_diskProvider.FolderExists(dlpath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFolderPermissions"), dlpath), "#permissions_error");
                    }

                    // if it's a remote client/docker, likely missing path mappings
                    if (_osInfo.IsDocker)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFolderPermissions"), client.Definition.Name, dlpath), "#docker_bad_remote_path_mapping");
                    }
                    else if (!status.IsLocalhost)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckRemoteDownloadClient"), client.Definition.Name, dlpath), "#bad_remote_path_mapping");
                    }
                    else
                    {
                        // path mappings shouldn't be needed locally so probably a permissions issue
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RemotePathMappingCheckFilesGenericPermissions"), client.Definition.Name, dlpath), "#permissions_error");
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in RemotePathMapping HealthCheck");
                }

                return new HealthCheck(GetType());
            }
            else
            {
                return Check();
            }
        }
    }
}
