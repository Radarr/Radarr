using System;
using System.IO;
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

        public MoviePathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
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

            return rootFolderPath.GetRelativePath(movie.Path);
        }
    }
}
