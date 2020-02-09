using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.DiskSpace
{
    public interface IDiskSpaceService
    {
        List<DiskSpace> GetFreeSpace();
    }

    public class DiskSpaceService : IDiskSpaceService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        private static readonly Regex _regexSpecialDrive = new Regex("^/var/lib/(docker|rancher|kubelet)(/|$)|^/(boot|etc)(/|$)|/docker(/var)?/aufs(/|$)", RegexOptions.Compiled);

        public DiskSpaceService(IDiskProvider diskProvider,
                                IRootFolderService rootFolderService,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public List<DiskSpace> GetFreeSpace()
        {
            var importantRootFolders = _rootFolderService.All().Select(x => x.Path).ToList();

            var optionalRootFolders = GetFixedDisksRootPaths().Except(importantRootFolders).Distinct().ToList();

            var diskSpace = GetDiskSpace(importantRootFolders).Concat(GetDiskSpace(optionalRootFolders, true)).ToList();

            return diskSpace;
        }

        private IEnumerable<string> GetFixedDisksRootPaths()
        {
            return _diskProvider.GetMounts()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Where(d => !_regexSpecialDrive.IsMatch(d.RootDirectory))
                .Select(d => d.RootDirectory);
        }

        private IEnumerable<DiskSpace> GetDiskSpace(IEnumerable<string> paths, bool suppressWarnings = false)
        {
            foreach (var path in paths)
            {
                DiskSpace diskSpace = null;

                try
                {
                    var freeSpace = _diskProvider.GetAvailableSpace(path);
                    var totalSpace = _diskProvider.GetTotalSize(path);

                    if (!freeSpace.HasValue || !totalSpace.HasValue)
                    {
                        continue;
                    }

                    diskSpace = new DiskSpace
                    {
                        Path = path,
                        FreeSpace = freeSpace.Value,
                        TotalSpace = totalSpace.Value
                    };

                    diskSpace.Label = _diskProvider.GetVolumeLabel(path);
                }
                catch (Exception ex)
                {
                    if (!suppressWarnings)
                    {
                        _logger.Warn(ex, "Unable to get free space for: " + path);
                    }
                }

                if (diskSpace != null)
                {
                    yield return diskSpace;
                }
            }
        }
    }
}
