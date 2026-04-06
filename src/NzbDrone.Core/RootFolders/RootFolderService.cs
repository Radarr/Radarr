using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        List<RootFolder> AllWithUnmappedFolders();
        RootFolder Add(RootFolder rootDir);
        void Remove(int id);
        RootFolder Get(int id, bool timeout);
        string GetBestRootFolderPath(string path, List<RootFolder> rootFolders = null);
    }

    public class RootFolderService : IRootFolderService
    {
        private readonly IRootFolderRepository _rootFolderRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly INamingConfigService _namingConfigService;
        private readonly Logger _logger;

        private readonly ICached<string> _cache;

        private static readonly HashSet<string> SpecialFolders = new HashSet<string>
                                                                 {
                                                                     "$recycle.bin",
                                                                     "system volume information",
                                                                     "recycler",
                                                                     "lost+found",
                                                                     ".appledb",
                                                                     ".appledesktop",
                                                                     ".appledouble",
                                                                     "@eadir",
                                                                     ".grab"
                                                                 };

        private static readonly Regex MovieFolderYearRegex = new Regex(@"\(\d{4}\)", RegexOptions.Compiled);

        public RootFolderService(IRootFolderRepository rootFolderRepository,
                                 IDiskProvider diskProvider,
                                 IMovieRepository movieRepository,
                                 IConfigService configService,
                                 INamingConfigService namingConfigService,
                                 ICacheManager cacheManager,
                                 Logger logger)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _movieRepository = movieRepository;
            _configService = configService;
            _namingConfigService = namingConfigService;
            _logger = logger;

            _cache = cacheManager.GetCache<string>(GetType());
        }

        public List<RootFolder> All()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            return rootFolders;
        }

        public List<RootFolder> AllWithUnmappedFolders()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            var moviePaths = _movieRepository.AllMoviePaths();

            rootFolders.ForEach(folder =>
            {
                try
                {
                    if (folder.Path.IsPathValid(PathValidationType.CurrentOs))
                    {
                        GetDetails(folder, moviePaths, true);
                    }
                }

                // We don't want an exception to prevent the root folders from loading in the UI, so they can still be deleted
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to get free space and unmapped folders for root folder {0}", folder.Path);
                    folder.UnmappedFolders = new List<UnmappedFolder>();
                }
            });

            return rootFolders;
        }

        public RootFolder Add(RootFolder rootFolder)
        {
            var all = All();

            if (string.IsNullOrWhiteSpace(rootFolder.Path) || !Path.IsPathRooted(rootFolder.Path))
            {
                throw new ArgumentException("Invalid path");
            }

            if (!_diskProvider.FolderExists(rootFolder.Path))
            {
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");
            }

            if (all.Exists(r => r.Path.PathEquals(rootFolder.Path)))
            {
                throw new InvalidOperationException("Recent directory already exists.");
            }

            if (!_diskProvider.FolderWritable(rootFolder.Path))
            {
                throw new UnauthorizedAccessException($"Root folder path '{rootFolder.Path}' is not writable by user '{Environment.UserName}'");
            }

            _rootFolderRepository.Insert(rootFolder);

            var moviePaths = _movieRepository.AllMoviePaths();

            GetDetails(rootFolder, moviePaths, true);
            _cache.Clear();

            return rootFolder;
        }

        public void Remove(int id)
        {
            _rootFolderRepository.Delete(id);
            _cache.Clear();
        }

        private List<UnmappedFolder> GetUnmappedFolders(string path, Dictionary<int, string> moviePaths)
        {
            _logger.Debug("Generating list of unmapped folders");

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid path provided", nameof(path));
            }

            var results = new List<UnmappedFolder>();

            if (!_diskProvider.FolderExists(path))
            {
                _logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            var possibleMovieFolders = GetPossibleMovieFolders(path);

            var unmappedFolders = possibleMovieFolders.Except(moviePaths.Select(s => s.Value), PathEqualityComparer.Instance).ToList();

            var recycleBinPath = _configService.RecycleBin;

            foreach (var unmappedFolder in unmappedFolders)
            {
                var di = new DirectoryInfo(unmappedFolder.Normalize());

                if ((!di.Attributes.HasFlag(FileAttributes.System) && !di.Attributes.HasFlag(FileAttributes.Hidden)) || di.Attributes.ToString() == "-1")
                {
                    if (string.IsNullOrWhiteSpace(recycleBinPath) || di.FullName.PathNotEquals(recycleBinPath))
                    {
                        results.Add(new UnmappedFolder
                        {
                            Name = di.Name,
                            Path = di.FullName,
                            RelativePath = path.GetRelativePath(di.FullName)
                        });
                    }
                }
            }

            var setToRemove = SpecialFolders;
            results.RemoveAll(x => setToRemove.Contains(new DirectoryInfo(x.Path.ToLowerInvariant()).Name));

            _logger.Debug("{0} unmapped folders detected.", results.Count);
            return results.OrderBy(u => u.Name, StringComparer.InvariantCultureIgnoreCase).ToList();
        }

        private List<string> GetPossibleMovieFolders(string path)
        {
            var subFolderDepth = _namingConfigService.GetConfig().MovieFolderFormat.Count(f => f == Path.DirectorySeparatorChar);
            var possibleMovieFolders = new List<string>();

            FindPossibleMovieFolders(path, 0, subFolderDepth, possibleMovieFolders);

            return possibleMovieFolders;
        }

        private void FindPossibleMovieFolders(string path, int currentDepth, int expectedMovieFolderDepth, List<string> possibleMovieFolders)
        {
            var directories = (_diskProvider.GetDirectories(path) ?? Enumerable.Empty<string>()).ToList();

            foreach (var directory in directories)
            {
                var childDirectories = (_diskProvider.GetDirectories(directory) ?? Enumerable.Empty<string>()).ToList();

                if (currentDepth < expectedMovieFolderDepth)
                {
                    FindPossibleMovieFolders(directory, currentDepth + 1, expectedMovieFolderDepth, possibleMovieFolders);
                    continue;
                }

                if (ShouldRecurseIntoGroupingFolder(directory, childDirectories))
                {
                    FindPossibleMovieFolders(directory, currentDepth + 1, expectedMovieFolderDepth, possibleMovieFolders);
                    continue;
                }

                if (IsPossibleMovieFolder(directory, childDirectories))
                {
                    possibleMovieFolders.Add(directory);
                }
            }
        }

        private bool IsPossibleMovieFolder(string path, List<string> childDirectories)
        {
            if (HasVideoFiles(path))
            {
                return true;
            }

            if (!childDirectories.Any())
            {
                return true;
            }

            return MovieFolderYearRegex.IsMatch(new DirectoryInfo(path).Name);
        }

        private bool ShouldRecurseIntoGroupingFolder(string path, List<string> childDirectories)
        {
            if (HasVideoFiles(path) || !childDirectories.Any())
            {
                return false;
            }

            var directoryName = new DirectoryInfo(path).Name;

            if (MovieFolderYearRegex.IsMatch(directoryName))
            {
                return false;
            }

            return childDirectories.All(ContainsMovieFolder);
        }

        private bool ContainsMovieFolder(string path)
        {
            if (HasVideoFiles(path))
            {
                return true;
            }

            return MovieFolderYearRegex.IsMatch(new DirectoryInfo(path).Name);
        }

        private bool HasVideoFiles(string path)
        {
            return (_diskProvider.GetFiles(path, false) ?? Enumerable.Empty<string>())
                                .Any(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)));
        }

        public RootFolder Get(int id, bool timeout)
        {
            var rootFolder = _rootFolderRepository.Get(id);
            var moviePaths = _movieRepository.AllMoviePaths();

            GetDetails(rootFolder, moviePaths, timeout);

            return rootFolder;
        }

        public string GetBestRootFolderPath(string path, List<RootFolder> rootFolders = null)
        {
            return _cache.Get(path, () => GetBestRootFolderPathInternal(path, rootFolders), TimeSpan.FromDays(1));
        }

        private void GetDetails(RootFolder rootFolder, Dictionary<int, string> moviePaths, bool timeout)
        {
            Task.Run(() =>
            {
                if (_diskProvider.FolderExists(rootFolder.Path))
                {
                    rootFolder.Accessible = true;
                    rootFolder.FreeSpace = _diskProvider.GetAvailableSpace(rootFolder.Path);
                    rootFolder.TotalSpace = _diskProvider.GetTotalSize(rootFolder.Path);
                    rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path, moviePaths);
                }
            }).Wait(timeout ? 5000 : -1);
        }

        private string GetBestRootFolderPathInternal(string path, List<RootFolder> rootFolders = null)
        {
            var allRootFoldersToConsider = rootFolders ?? All();

            var possibleRootFolder = allRootFoldersToConsider.Where(r => r.Path.IsParentPath(path)).MaxBy(r => r.Path.Length);

            if (possibleRootFolder == null)
            {
                var osPath = new OsPath(path);

                return osPath.Directory.ToString().GetCleanPath();
            }

            return possibleRootFolder.Path.GetCleanPath();
        }
    }
}
