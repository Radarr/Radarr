using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Files;

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
        public abstract IEnumerable<ExtraFile> ProcessFiles(Author author, List<string> filesOnDisk, List<string> importedFiles);

        public virtual ImportExistingExtraFileFilterResult<TExtraFile> FilterAndClean(Author author, List<string> filesOnDisk, List<string> importedFiles)
        {
            var authorFiles = _extraFileService.GetFilesByAuthor(author.Id);

            Clean(author, filesOnDisk, importedFiles, authorFiles);

            return Filter(author, filesOnDisk, importedFiles, authorFiles);
        }

        private ImportExistingExtraFileFilterResult<TExtraFile> Filter(Author author, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> authorFiles)
        {
            var previouslyImported = authorFiles.IntersectBy(s => Path.Combine(author.Path, s.RelativePath), filesOnDisk, f => f, PathEqualityComparer.Instance).ToList();
            var filteredFiles = filesOnDisk.Except(previouslyImported.Select(f => Path.Combine(author.Path, f.RelativePath)).ToList(), PathEqualityComparer.Instance)
                                           .Except(importedFiles, PathEqualityComparer.Instance)
                                           .ToList();

            // Return files that are already imported so they aren't imported again by other importers.
            // Filter out files that were previously imported and as well as ones imported by other importers.
            return new ImportExistingExtraFileFilterResult<TExtraFile>(previouslyImported, filteredFiles);
        }

        private void Clean(Author author, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> authorFiles)
        {
            var alreadyImportedFileIds = authorFiles.IntersectBy(f => Path.Combine(author.Path, f.RelativePath), importedFiles, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            var deletedFiles = authorFiles.ExceptBy(f => Path.Combine(author.Path, f.RelativePath), filesOnDisk, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            _extraFileService.DeleteMany(alreadyImportedFileIds);
            _extraFileService.DeleteMany(deletedFiles);
        }
    }
}
