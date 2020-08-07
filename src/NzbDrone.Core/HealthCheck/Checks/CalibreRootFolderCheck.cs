using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ModelEvent<RootFolder>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]
    public class CalibreRootFolderCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly ICalibreProxy _calibreProxy;
        private readonly Logger _logger;
        private readonly IOsInfo _osInfo;

        public CalibreRootFolderCheck(IDiskProvider diskProvider,
                                      IRootFolderService rootFolderService,
                                      ICalibreProxy calibreProxy,
                                      IOsInfo osInfo,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
            _calibreProxy = calibreProxy;
            _logger = logger;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            var rootFolders = _rootFolderService.All().Where(x => x.IsCalibreLibrary);

            foreach (var folder in rootFolders)
            {
                try
                {
                    var calibreIsLocal = folder.CalibreSettings.Host == "127.0.0.1" || folder.CalibreSettings.Host == "localhost";

                    var files = _calibreProxy.GetAllBookFilePaths(folder.CalibreSettings);
                    if (files.Any())
                    {
                        var file = files.First();

                        // This directory structure is forced by calibre
                        var bookFolder = Path.GetDirectoryName(file);
                        var authorFolder = Path.GetDirectoryName(bookFolder);
                        var libraryFolder = Path.GetDirectoryName(authorFolder);

                        var osPath = new OsPath(libraryFolder);

                        if (!osPath.IsValid)
                        {
                            if (!calibreIsLocal)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote calibre for root folder {folder.Name} reports files in {libraryFolder} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and root folder settings.", "#bad-remote-path-mapping");
                            }
                            else if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; calibre for root folder {folder.Name} reports files in {libraryFolder} but this is not a valid {_osInfo.Name} path.  Review your remote path mappings and download client settings.", "#docker-bad-remote-path-mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Local calibre server for root folder {folder.Name} reports files in {libraryFolder} but this is not a valid {_osInfo.Name} path.  Review your download client settings.", "#bad-download-client-settings");
                            }
                        }

                        if (!_diskProvider.FolderExists(libraryFolder))
                        {
                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; calibre server for root folder {folder.Name} places downloads in {libraryFolder} but this directory does not appear to exist inside the container.  Review your remote path mappings and container volume settings.", "#docker-bad-remote-path-mapping");
                            }
                            else if (!calibreIsLocal)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote calibre server for root folder {folder.Name} places downloads in {libraryFolder} but this directory does not appear to exist.  Likely missing or incorrect remote path mapping.", "#bad-remote-path-mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Calibre server for root folder {folder.Name} places downloads in {libraryFolder} but Readarr cannot see this directory.  You may need to adjust the folder's permissions or add a remote path mapping if calibre is running in docker", "#permissions-error");
                            }
                        }

                        if (!_diskProvider.FileExists(file))
                        {
                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"You are using docker; calibre server for root folder {folder.Name} listed file {file} but this file does not appear to exist inside the container.  Review permissions for {libraryFolder} and PUID/PGID container settings", "#docker-bad-remote-path-mapping");
                            }
                            else if (!calibreIsLocal)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Remote calibre server for root folder {folder.Name} listed file {file} but this file does not appear to exist.  Review permissions for {libraryFolder}", "#permissions-error");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Calibre server for root folder {folder.Name} listed file {file} but Readarr cannot see this file.  Review permissions for {libraryFolder}", "#permissions-error");
                            }
                        }

                        if (!libraryFolder.PathEquals(folder.Path))
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, $"Calibre for root folder {folder.Name} reports files in {libraryFolder} but this is not the same as the root folder path {folder.Path} you chose.  You may need to edit any remote path mapping or delete the root folder and re-create with the correct path", "#calibre-root-does-not-match");
                        }
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with calibre server for root folder {0}", folder.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in CalibreRootFolderCheck HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
