using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderAddedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]
    [CheckOn(typeof(TrackImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(TrackImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RemotePathMappingCheck : HealthCheckBase, IProvideHealthCheckWithMessage
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;
        private readonly IOsInfo _osInfo;

        public RemotePathMappingCheck(IDiskProvider diskProvider,
                                      IProvideDownloadClient downloadClientProvider,
                                      IOsInfo osInfo,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            var clients = _downloadClientProvider.GetDownloadClients();

            foreach (var client in clients)
            {
                try
                {
                    var status = client.GetStatus();
                    var folders = status.OutputRootFolders;
                    if (folders != null)
                    {
                        foreach (var folder in folders)
                        {
                            if (!folder.IsValid)
                            {
                                if (!status.IsLocalhost)
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote download client {client.Definition.Name} places downloads in {folder.FullPath} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and download client settings.", "#bad-remote-path-mapping");
                                }
                                else if (_osInfo.IsDocker)
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; download client {client.Definition.Name} places downloads in {folder.FullPath} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and download client settings.", "#docker-bad-remote-path-mapping");
                                }
                                else
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Local download client {client.Definition.Name} places downloads in {folder.FullPath} but this is not a valid {_osInfo.Name} path.  Review your download client settings.", "#bad-download-client-settings");
                                }
                            }

                            if (!_diskProvider.FolderExists(folder.FullPath))
                            {
                                if (_osInfo.IsDocker)
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; download client {client.Definition.Name} places downloads in {folder.FullPath} but this directory does not appear to exist inside the container.  Review your remote path mappings and container volume settings.", "#docker-bad-remote-path-mapping");
                                }
                                else if (!status.IsLocalhost)
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote download client {client.Definition.Name} places downloads in {folder.FullPath} but this directory does not appear to exist.  Likely missing or incorrect remote path mapping.", "#bad-remote-path-mapping");
                                }
                                else
                                {
                                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Download client {client.Definition.Name} places downloads in {folder.FullPath} but Lidarr cannot see this directory.  You may need to adjust the folder's permissions.", "#permissions-error");
                                }
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
            if (typeof(TrackImportFailedEvent).IsAssignableFrom(message.GetType()))
            {
                var failureMessage = (TrackImportFailedEvent) message;

                // if we can see the file exists but the import failed then likely a permissions issue
                if (failureMessage.TrackInfo != null)
                {
                    var trackPath = failureMessage.TrackInfo.Path;
                    if (_diskProvider.FileExists(trackPath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Lidarr can see but not access downloaded track {trackPath}.  Likely permissions error.", "#permissions-error");
                    }
                    else
                    {
                        // If the file doesn't exist but TrackInfo is not null then the message is coming from
                        // ImportApprovedTracks and the file must have been removed part way through processing
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"File {trackPath} was removed part way though procesing.");
                    }
                }

                // If the previous case did not match then the failure occured in DownloadedTracksImportService,
                // while trying to locate the files reported by the download client
                var client = _downloadClientProvider.GetDownloadClients().FirstOrDefault(x => x.Definition.Name == failureMessage.DownloadClient);
                try
                {
                    var status = client.GetStatus();
                    var dlpath = client?.GetItems().FirstOrDefault(x => x.DownloadId == failureMessage.DownloadId)?.OutputPath.FullPath;

                    // If dlpath is null then there's not much useful we can report.  Give a generic message so
                    // that the user realises something is wrong.
                    if (dlpath.IsNullOrWhiteSpace())
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Lidarr failed to import a track.  Check your logs for details.");
                    }

                    if (!dlpath.IsPathValid())
                    {
                        if (!status.IsLocalhost)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote download client {client.Definition.Name} reported files in {dlpath} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and download client settings.", "#bad-remote-path-mapping");
                        }
                        else if (_osInfo.IsDocker)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; download client {client.Definition.Name} reported files in {dlpath} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and download client settings.", "#docker-bad-remote-path-mapping");
                        }
                        else
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, $"Local download client {client.Definition.Name} reported files in {dlpath} but this is not a valid {_osInfo.Name} path.  Review your download client settings.", "#bad-download-client-settings");
                        }
                    }

                    if (_diskProvider.FolderExists(dlpath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Lidarr can see but not access download directory {dlpath}.  Likely permissions error.", "#permissions-error");
                    }
                
                    // if it's a remote client/docker, likely missing path mappings
                    if (_osInfo.IsDocker)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; download client {client.Definition.Name} reported files in {dlpath} but this directory does not appear to exist inside the container.  Review your remote path mappings and container volume settings.", "#docker-bad-remote-path-mapping");
                    }
                    else if (!status.IsLocalhost)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote download client {client.Definition.Name} reported files in {dlpath} but this directory does not appear to exist.  Likely missing remote path mapping.", "#bad-remote-path-mapping");
                    }
                    else
                    {
                        // path mappings shouldn't be needed locally so probably a permissions issue
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Download client {client.Definition.Name} reported files in {dlpath} but Lidarr cannot see this directory.  You may need to adjust the folder's permissions.", "#permissions-error");
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
