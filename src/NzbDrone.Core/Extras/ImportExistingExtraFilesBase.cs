using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras
{
    public abstract class ImportExistingExtraFilesBase<TExtraFile> : IImportExistingExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IExtraFileService<TExtraFile> _extraFileService;

        public ImportExistingExtraFilesBase(IExtraFileService<TExtraFile> extraFileService)
        {
            _extraFileService = extraFileService;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles);

        public virtual ImportExistingExtraFileFilterResult<TExtraFile> FilterAndClean(Artist artist, List<string> filesOnDisk, List<string> importedFiles)
        {
            var artistFiles = _extraFileService.GetFilesByArtist(artist.Id);

            Clean(artist, filesOnDisk, importedFiles, artistFiles);

            return Filter(artist, filesOnDisk, importedFiles, artistFiles);
        }

        private ImportExistingExtraFileFilterResult<TExtraFile> Filter(Artist artist, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> artistFiles)
        {
            var previouslyImported = artistFiles.IntersectBy(s => Path.Combine(artist.Path, s.RelativePath), filesOnDisk, f => f, PathEqualityComparer.Instance).ToList();
            var filteredFiles = filesOnDisk.Except(previouslyImported.Select(f => Path.Combine(artist.Path, f.RelativePath)).ToList(), PathEqualityComparer.Instance)
                                           .Except(importedFiles, PathEqualityComparer.Instance)
                                           .ToList();

            // Return files that are already imported so they aren't imported again by other importers.
            // Filter out files that were previously imported and as well as ones imported by other importers.
            return new ImportExistingExtraFileFilterResult<TExtraFile>(previouslyImported, filteredFiles);
        }

        private void Clean(Artist artist, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> artistFiles)
        {
            var alreadyImportedFileIds = artistFiles.IntersectBy(f => Path.Combine(artist.Path, f.RelativePath), importedFiles, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            var deletedFiles = artistFiles.ExceptBy(f => Path.Combine(artist.Path, f.RelativePath), filesOnDisk, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            _extraFileService.DeleteMany(alreadyImportedFileIds);
            _extraFileService.DeleteMany(deletedFiles);
        }
    }
}
