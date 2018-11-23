using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.DiskSpace
{
    public interface IDiskSpaceService
    {
        List<DiskSpace> GetFreeSpace();
    }

    public class DiskSpaceService : IDiskSpaceService
    {
        private readonly IMovieService _movieService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private static readonly Regex _regexSpecialDrive = new Regex("^/var/lib/(docker|rancher|kubelet)(/|$)|^/(boot|etc|snap)(/|$)|/docker(/var)?/aufs(/|$)", RegexOptions.Compiled);

        public DiskSpaceService(IMovieService movieService, IDiskProvider diskProvider, Logger logger)
        {
            _movieService = movieService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<DiskSpace> GetFreeSpace()
        {
            var diskSpace = new List<DiskSpace>();
            diskSpace.AddRange(GetMovieFreeSpace());
            diskSpace.AddRange(GetFixedDisksFreeSpace());

            return diskSpace.DistinctBy(d => d.Path).ToList();
        }

        private IEnumerable<DiskSpace> GetMovieFreeSpace()
        {
            var movieRootPaths = _movieService.GetAllMovies().Select(s => _diskProvider.GetPathRoot(s.Path)).Distinct();

            return GetDiskSpace(movieRootPaths);
        }

        private IEnumerable<DiskSpace> GetFixedDisksFreeSpace()
        {
            return GetDiskSpace(_diskProvider.GetMounts()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Where(d => !_regexSpecialDrive.IsMatch(d.RootDirectory))
                .Select(d => d.RootDirectory), true);
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
