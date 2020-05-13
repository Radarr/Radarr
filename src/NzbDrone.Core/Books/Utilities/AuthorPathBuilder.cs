using System;
using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Books
{
    public interface IBuildAuthorPaths
    {
        string BuildPath(Author author, bool useExistingRelativeFolder);
    }

    public class AuthorPathBuilder : IBuildAuthorPaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;

        public AuthorPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
        }

        public string BuildPath(Author author, bool useExistingRelativeFolder)
        {
            if (author.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(author));
            }

            if (useExistingRelativeFolder && author.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(author);
                return Path.Combine(author.RootFolderPath, relativePath);
            }

            return Path.Combine(author.RootFolderPath, _fileNameBuilder.GetAuthorFolder(author));
        }

        private string GetExistingRelativePath(Author author)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(author.Path);

            return rootFolderPath.GetRelativePath(author.Path);
        }
    }
}
