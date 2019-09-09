using System;
using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Music
{
    public interface IBuildArtistPaths
    {
        string BuildPath(Artist artist, bool useExistingRelativeFolder);
    }

    public class ArtistPathBuilder : IBuildArtistPaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;

        public ArtistPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
        }

        public string BuildPath(Artist artist, bool useExistingRelativeFolder)
        {
            if (artist.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(artist));
            }

            if (useExistingRelativeFolder && artist.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(artist);
                return Path.Combine(artist.RootFolderPath, relativePath);
            }

            return Path.Combine(artist.RootFolderPath, _fileNameBuilder.GetArtistFolder(artist));
        }

        private string GetExistingRelativePath(Artist artist)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(artist.Path);

            return rootFolderPath.GetRelativePath(artist.Path);
        }
    }
}
