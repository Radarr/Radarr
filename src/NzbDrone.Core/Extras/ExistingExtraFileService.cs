using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras
{
    public class ExistingExtraFileService : IHandle<MovieScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly List<IImportExistingExtraFiles> _existingExtraFileImporters;
        private readonly Logger _logger;

        public ExistingExtraFileService(IDiskProvider diskProvider,
                                        IDiskScanService diskScanService,
                                        List<IImportExistingExtraFiles> existingExtraFileImporters,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _existingExtraFileImporters = existingExtraFileImporters.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public void Handle(MovieScannedEvent message)
        {
            var movie = message.Movie;
            var extraFiles = new List<ExtraFile>();

            if (!_diskProvider.FolderExists(movie.Path))
            {
                return;
            }

            _logger.Debug("Looking for existing extra files in {0}", movie.Path);

            var filesOnDisk = _diskScanService.GetNonVideoFiles(movie.Path);
            var possibleExtraFiles = _diskScanService.FilterPaths(movie.Path, filesOnDisk, false);

            var filteredFiles = possibleExtraFiles;
            var importedFiles = new List<string>();

            foreach (var existingExtraFileImporter in _existingExtraFileImporters)
            {
                var imported = existingExtraFileImporter.ProcessFiles(movie, filteredFiles, importedFiles);

                importedFiles.AddRange(imported.Select(f => Path.Combine(movie.Path, f.RelativePath)));
            }

            _logger.Info("Found {0} extra files", extraFiles.Count);
        }
    }
}
