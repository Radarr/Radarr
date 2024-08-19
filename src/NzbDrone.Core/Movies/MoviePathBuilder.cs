using System;
using System.IO;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public interface IBuildMoviePaths
    {
        string BuildPath(Movie movie, bool useExistingRelativeFolder);
    }

    public class MoviePathBuilder : IBuildMoviePaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public MoviePathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService, Logger logger)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public string BuildPath(Movie movie, bool useExistingRelativeFolder)
        {
            if (movie.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(movie));
            }

            if (useExistingRelativeFolder && movie.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(movie);
                return Path.Combine(movie.RootFolderPath, relativePath);
            }

            return Path.Combine(movie.RootFolderPath, _fileNameBuilder.GetMovieFolder(movie));
        }

        private string GetExistingRelativePath(Movie movie)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(movie.Path);

            if (rootFolderPath.IsParentPath(movie.Path))
            {
                return rootFolderPath.GetRelativePath(movie.Path);
            }

            var directoryName = movie.Path.GetDirectoryName();

            _logger.Warn("Unable to get relative path for movie path {0}, using movie folder name {1}", movie.Path, directoryName);

            return directoryName;
        }
    }
}
