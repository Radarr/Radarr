using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras
{
    public interface IExistingExtraFiles
    {
        List<string> ImportExtraFiles(Movie movie, List<string> possibleExtraFiles, string fileNameBeforeRename);
    }

    public class ExistingExtraFileService : IExistingExtraFiles, IHandle<MovieScannedEvent>
    {
        private readonly List<IImportExistingExtraFiles> _existingExtraFileImporters;
        private readonly Logger _logger;

        public ExistingExtraFileService(IEnumerable<IImportExistingExtraFiles> existingExtraFileImporters,
                                        Logger logger)
        {
            _existingExtraFileImporters = existingExtraFileImporters.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public List<string> ImportExtraFiles(Movie movie, List<string> possibleExtraFiles, string fileNameBeforeRename)
        {
            _logger.Debug("Looking for existing extra files in {0}", movie.Path);

            var importedFiles = new List<string>();

            foreach (var existingExtraFileImporter in _existingExtraFileImporters)
            {
                var imported = existingExtraFileImporter.ProcessFiles(movie, possibleExtraFiles, importedFiles, fileNameBeforeRename);

                importedFiles.AddRange(imported.Select(f => Path.Combine(movie.Path, f.RelativePath)));
            }

            return importedFiles;
        }

        public void Handle(MovieScannedEvent message)
        {
            var movie = message.Movie;
            var possibleExtraFiles = message.PossibleExtraFiles;
            var importedFiles = ImportExtraFiles(movie, possibleExtraFiles, null);

            _logger.Info("Found {0} possible extra files, imported {1} files.", possibleExtraFiles.Count, importedFiles.Count);
        }
    }
}
