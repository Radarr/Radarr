using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            // Only check clients not in failure status, those get another message
            var clients = _downloadClientProvider.GetDownloadClients(true);

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
                                return new HealthCheck(
                                    GetType(),
                                    HealthCheckResult.Error,
                                    _localizationService.GetLocalizedString(
                                        "RemotePathMappingCheckWrongOSPath", new Dictionary<string, object>
                                        {
                                            { "downloadClientName", client.Definition.Name },
                                            { "path", folder.FullPath },
                                            { "osName", _osInfo.Name }
                                        }),
                                    "#bad-remote-path-mapping");
                            }

                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(
                                    GetType(),
                                    HealthCheckResult.Error,
                                    _localizationService.GetLocalizedString(
                                        "RemotePathMappingCheckBadDockerPath",
                                        new Dictionary<string, object>
                                        {
                                            { "downloadClientName", client.Definition.Name },
                                            { "path", folder.FullPath },
                                            { "osName", _osInfo.Name }
                                        }),
                                    "#docker-bad-remote-path-mapping");
                            }

                            return new HealthCheck(
                                GetType(),
                                HealthCheckResult.Error,
                                _localizationService.GetLocalizedString(
                                    "RemotePathMappingCheckLocalWrongOSPath",
                                    new Dictionary<string, object>
                                    {
                                        { "downloadClientName", client.Definition.Name },
                                        { "path", folder.FullPath },
                                        { "osName", _osInfo.Name }
                                    }),
                                "#bad-download-client-settings");
                        }

                        if (!_diskProvider.FolderExists(folder.FullPath))
                        {
                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(
                                    GetType(),
                                    HealthCheckResult.Error,
                                    _localizationService.GetLocalizedString(
                                        "RemotePathMappingCheckDockerFolderMissing",
                                        new Dictionary<string, object>
                                        {
                                            { "downloadClientName", client.Definition.Name },
                                            { "path", folder.FullPath }
                                        }),
                                    "#docker-bad-remote-path-mapping");
                            }

                            if (!status.IsLocalhost)
                            {
                                return new HealthCheck(
                                    GetType(),
                                    HealthCheckResult.Error,
                                    _localizationService.GetLocalizedString(
                                        "RemotePathMappingCheckLocalFolderMissing",
                                        new Dictionary<string, object>
                                        {
                                            { "downloadClientName", client.Definition.Name },
                                            { "path", folder.FullPath }
                                        }),
                                    "#bad-remote-path-mapping");
                            }

                            return new HealthCheck(
                                GetType(),
                                HealthCheckResult.Error,
                                _localizationService.GetLocalizedString(
                                    "RemotePathMappingCheckGenericPermissions",
                                    new Dictionary<string, object>
                                    {
                                        { "downloadClientName", client.Definition.Name },
                                        { "path", folder.FullPath }
                                    }),
                                "#permissions-error");
                        }
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (HttpRequestException ex)
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

            if (message is MovieImportFailedEvent failureMessage)
            {
                // if we can see the file exists but the import failed then likely a permissions issue
                if (failureMessage.MovieInfo != null)
                {
                    var moviePath = failureMessage.MovieInfo.Path;

                    if (_diskProvider.FileExists(moviePath))
                    {
                        return new HealthCheck(GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString(
                                "RemotePathMappingCheckDownloadPermissions",
                                new Dictionary<string, object>
                                {
                                    { "path", moviePath }
                                }),
                            "#permissions-error");
                    }

                    // If the file doesn't exist but MovieInfo is not null then the message is coming from
                    // ImportApprovedMovies and the file must have been removed part way through processing
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        _localizationService.GetLocalizedString(
                            "RemotePathMappingCheckFileRemoved",
                            new Dictionary<string, object>
                            {
                                { "path", moviePath }
                            }),
                        "#remote-path-file-removed");
                }

                // If the previous case did not match then the failure occured in DownloadedMovieImportService,
                // while trying to locate the files reported by the download client
                // Only check clients not in failure status, those get another message
                var client = _downloadClientProvider.GetDownloadClients(true).FirstOrDefault(x => x.Definition.Name == failureMessage.DownloadClientInfo.Name);

                if (client == null)
                {
                    return new HealthCheck(GetType());
                }

                try
                {
                    var status = client.GetStatus();
                    var dlpath = client?.GetItems().FirstOrDefault(x => x.DownloadId == failureMessage.DownloadId)?.OutputPath.FullPath;

                    // If dlpath is null then there's not much useful we can report.  Give a generic message so
                    // that the user realises something is wrong.
                    if (dlpath.IsNullOrWhiteSpace())
                    {
                        return new HealthCheck(
                            GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString("RemotePathMappingCheckImportFailed"),
                            "#remote-path-import-failed");
                    }

                    if (!dlpath.IsPathValid(PathValidationType.CurrentOs))
                    {
                        if (!status.IsLocalhost)
                        {
                            return new HealthCheck(
                                GetType(),
                                HealthCheckResult.Error,
                                _localizationService.GetLocalizedString(
                                    "RemotePathMappingCheckFilesWrongOSPath",
                                    new Dictionary<string, object>
                                    {
                                        { "downloadClientName", client.Definition.Name },
                                        { "path", dlpath },
                                        { "osName", _osInfo.Name }
                                    }),
                                "#bad-remote-path-mapping");
                        }

                        if (_osInfo.IsDocker)
                        {
                            return new HealthCheck(
                                GetType(),
                                HealthCheckResult.Error,
                                _localizationService.GetLocalizedString(
                                    "RemotePathMappingCheckFilesBadDockerPath",
                                    new Dictionary<string, object>
                                    {
                                        { "downloadClientName", client.Definition.Name },
                                        { "path", dlpath },
                                        { "osName", _osInfo.Name }
                                    }),
                                "#docker-bad-remote-path-mapping");
                        }

                        return new HealthCheck(
                            GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString(
                                "RemotePathMappingCheckFilesLocalWrongOSPath",
                                new Dictionary<string, object>
                                {
                                    { "downloadClientName", client.Definition.Name },
                                    { "path", dlpath },
                                    { "osName", _osInfo.Name }
                                }),
                            "#bad-download-client-settings");
                    }

                    if (_diskProvider.FolderExists(dlpath))
                    {
                        return new HealthCheck(
                            GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString(
                                "RemotePathMappingCheckFolderPermissions",
                                new Dictionary<string, object>
                                {
                                    { "path", dlpath }
                                }),
                            "#permissions-error");
                    }

                    // if it's a remote client/docker, likely missing path mappings
                    if (_osInfo.IsDocker)
                    {
                        return new HealthCheck(
                            GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString(
                                "RemotePathMappingCheckFolderPermissions",
                                new Dictionary<string, object>
                                {
                                    { "downloadClientName", client.Definition.Name },
                                    { "path", dlpath }
                                }),
                            "#docker-bad-remote-path-mapping");
                    }

                    if (!status.IsLocalhost)
                    {
                        return new HealthCheck(
                            GetType(),
                            HealthCheckResult.Error,
                            _localizationService.GetLocalizedString(
                                "RemotePathMappingCheckRemoteDownloadClient",
                                new Dictionary<string, object>
                                {
                                    { "downloadClientName", client.Definition.Name },
                                    { "path", dlpath },
                                    { "osName", _osInfo.Name }
                                }), "#bad-remote-path-mapping");
                    }

                    // path mappings shouldn't be needed locally so probably a permissions issue
                    return new HealthCheck(
                        GetType(),
                        HealthCheckResult.Error,
                        _localizationService.GetLocalizedString(
                            "RemotePathMappingCheckFilesGenericPermissions",
                            new Dictionary<string, object>
                            {
                                { "downloadClientName", client.Definition.Name },
                                { "path", dlpath }
                            }),
                        "#permissions-error");
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in RemotePathMapping HealthCheck");
                }

                return new HealthCheck(GetType());
            }

            return Check();
        }
    }
}
