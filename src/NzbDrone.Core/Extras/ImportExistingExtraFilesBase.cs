using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Movies;

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
        public abstract IEnumerable<ExtraFile> ProcessFiles(Movie movie, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename);

        public virtual ImportExistingExtraFileFilterResult<TExtraFile> FilterAndClean(Movie movie, List<string> filesOnDisk, List<string> importedFiles, bool keepExistingEntries)
        {
            var movieFiles = _extraFileService.GetFilesByMovie(movie.Id);

            if (keepExistingEntries)
            {
                var incompleteImports = movieFiles.IntersectBy(f => Path.Combine(movie.Path, f.RelativePath), filesOnDisk, i => i, PathEqualityComparer.Instance).Select(f => f.Id);

                _extraFileService.DeleteMany(incompleteImports);

                return Filter(movie, filesOnDisk, importedFiles, new List<TExtraFile>());
            }

            Clean(movie, filesOnDisk, importedFiles, movieFiles);

            return Filter(movie, filesOnDisk, importedFiles, movieFiles);
        }

        private ImportExistingExtraFileFilterResult<TExtraFile> Filter(Movie movie, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> movieFiles)
        {
            var previouslyImported = movieFiles.IntersectBy(s => Path.Combine(movie.Path, s.RelativePath), filesOnDisk, f => f, PathEqualityComparer.Instance).ToList();
            var filteredFiles = filesOnDisk.Except(previouslyImported.Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList(), PathEqualityComparer.Instance)
                                           .Except(importedFiles, PathEqualityComparer.Instance)
                                           .ToList();

            // Return files that are already imported so they aren't imported again by other importers.
            // Filter out files that were previously imported and as well as ones imported by other importers.
            return new ImportExistingExtraFileFilterResult<TExtraFile>(previouslyImported, filteredFiles);
        }

        private void Clean(Movie movie, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> movieFiles)
        {
            var alreadyImportedFileIds = movieFiles.IntersectBy(f => Path.Combine(movie.Path, f.RelativePath), importedFiles, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            var deletedFiles = movieFiles.ExceptBy(f => Path.Combine(movie.Path, f.RelativePath), filesOnDisk, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            _extraFileService.DeleteMany(alreadyImportedFileIds);
            _extraFileService.DeleteMany(deletedFiles);
        }
    }
}
