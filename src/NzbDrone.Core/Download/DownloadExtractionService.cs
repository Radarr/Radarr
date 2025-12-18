using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Download
{
    public interface IDownloadExtractionService
    {
        string ExtractIfNeeded(string downloadPath);
        bool ShouldExtract(string downloadPath);
    }

    public class DownloadExtractionService : IDownloadExtractionService
    {
        private readonly IArchiveService _archiveService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadExtractionService(
            IArchiveService archiveService,
            IDiskProvider diskProvider,
            IConfigService configService,
            Logger logger)
        {
            _archiveService = archiveService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public bool ShouldExtract(string downloadPath)
        {
            if (!_configService.AutoExtractArchives)
            {
                return false;
            }

            if (!_diskProvider.FolderExists(downloadPath) && !_diskProvider.FileExists(downloadPath))
            {
                return false;
            }

            if (_diskProvider.FileExists(downloadPath))
            {
                return _archiveService.IsArchive(downloadPath);
            }

            var files = _diskProvider.GetFiles(downloadPath, true);
            return files.Any(f => _archiveService.IsArchive(f));
        }

        public string ExtractIfNeeded(string downloadPath)
        {
            if (!ShouldExtract(downloadPath))
            {
                return downloadPath;
            }

            try
            {
                if (_diskProvider.FileExists(downloadPath))
                {
                    return ExtractSingleArchive(downloadPath);
                }

                return ExtractArchivesInFolder(downloadPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to extract archives in {0}", downloadPath);
                return downloadPath;
            }
        }

        private string ExtractSingleArchive(string archivePath)
        {
            var extractionPath = Path.Combine(
                Path.GetDirectoryName(archivePath),
                Path.GetFileNameWithoutExtension(archivePath));

            _logger.Info("Extracting {0} to {1}", archivePath, extractionPath);

            _archiveService.Extract(archivePath, extractionPath);

            if (_configService.DeleteArchiveAfterExtraction)
            {
                _logger.Debug("Deleting archive after extraction: {0}", archivePath);
                _diskProvider.DeleteFile(archivePath);
            }

            return extractionPath;
        }

        private string ExtractArchivesInFolder(string folderPath)
        {
            var files = _diskProvider.GetFiles(folderPath, true);
            var archiveFiles = files
                .Where(f => _archiveService.IsArchive(f))
                .Where(f => !IsPartOfMultiVolumeArchive(f))
                .ToList();

            if (!archiveFiles.Any())
            {
                return folderPath;
            }

            foreach (var archiveFile in archiveFiles)
            {
                var extractionPath = Path.GetDirectoryName(archiveFile);
                _logger.Info("Extracting {0}", archiveFile);

                try
                {
                    _archiveService.Extract(archiveFile, extractionPath);

                    if (_configService.DeleteArchiveAfterExtraction)
                    {
                        DeleteArchiveWithParts(archiveFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to extract {0}", archiveFile);
                }
            }

            return folderPath;
        }

        private bool IsPartOfMultiVolumeArchive(string path)
        {
            var extension = Path.GetExtension(path);

            if (extension.StartsWith(".r", StringComparison.OrdinalIgnoreCase) &&
                extension.Length == 4 &&
                int.TryParse(extension.Substring(2), out _))
            {
                return true;
            }

            if (path.Contains(".part") && extension.Equals(".rar", StringComparison.OrdinalIgnoreCase))
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                return filename.EndsWith(".part1", StringComparison.OrdinalIgnoreCase) == false;
            }

            return false;
        }

        private void DeleteArchiveWithParts(string archivePath)
        {
            var directory = Path.GetDirectoryName(archivePath);
            var baseName = Path.GetFileNameWithoutExtension(archivePath);

            var partFiles = _diskProvider.GetFiles(directory, false)
                .Where(f => IsArchivePartFile(f, baseName))
                .ToList();

            foreach (var partFile in partFiles)
            {
                _logger.Debug("Deleting archive part: {0}", partFile);
                _diskProvider.DeleteFile(partFile);
            }

            _diskProvider.DeleteFile(archivePath);
        }

        private bool IsArchivePartFile(string filePath, string baseName)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            if (fileName.Equals(baseName, StringComparison.OrdinalIgnoreCase) &&
                extension.StartsWith(".r", StringComparison.OrdinalIgnoreCase) &&
                extension.Length == 4)
            {
                return true;
            }

            if (fileName.StartsWith(baseName + ".part", StringComparison.OrdinalIgnoreCase) &&
                extension.Equals(".rar", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
