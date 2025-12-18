using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using NLog;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace NzbDrone.Common
{
    public interface IArchiveService
    {
        void Extract(string compressedFile, string destination);
        void CreateZip(string path, IEnumerable<string> files);
        bool IsArchive(string path);
        bool CanExtract(string path);
    }

    public class ArchiveService : IArchiveService
    {
        private readonly Logger _logger;

        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".rar", ".7z", ".tar", ".gz", ".tgz", ".tar.gz", ".bz2", ".tar.bz2"
        };

        public ArchiveService(Logger logger)
        {
            _logger = logger;
        }

        public bool IsArchive(string path)
        {
            var extension = Path.GetExtension(path);
            return SupportedExtensions.Contains(extension) ||
                   path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanExtract(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            return IsArchive(path);
        }

        public void Extract(string compressedFile, string destination)
        {
            _logger.Debug("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            var extension = Path.GetExtension(compressedFile).ToLowerInvariant();

            if (extension == ".zip")
            {
                ExtractZip(compressedFile, destination);
            }
            else if (extension == ".rar" || extension == ".7z" || extension == ".r00")
            {
                ExtractWithSharpCompress(compressedFile, destination);
            }
            else if (compressedFile.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                     compressedFile.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase) ||
                     extension == ".tar" || extension == ".gz")
            {
                ExtractTgz(compressedFile, destination);
            }
            else
            {
                ExtractWithSharpCompress(compressedFile, destination);
            }

            _logger.Debug("Extraction complete.");
        }

        private void ExtractWithSharpCompress(string compressedFile, string destination)
        {
            var destinationFullPath = Path.GetFullPath(destination);
            Directory.CreateDirectory(destination);

            using var archive = ArchiveFactory.Open(compressedFile);
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(Path.Combine(destination, entry.Key));

                if (!fullPath.StartsWith(destinationFullPath + Path.DirectorySeparatorChar) &&
                    !fullPath.Equals(destinationFullPath, StringComparison.Ordinal))
                {
                    _logger.Warn("Skipping archive entry with path traversal attempt: {0}", entry.Key);
                    continue;
                }

                var directoryName = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                entry.WriteToFile(fullPath, new ExtractionOptions
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
            }
        }

        public void CreateZip(string path, IEnumerable<string> files)
        {
            _logger.Debug("Creating archive {0}", path);

            using var zipFile = ZipFile.Create(path);

            zipFile.BeginUpdate();

            foreach (var file in files)
            {
                zipFile.Add(file, Path.GetFileName(file));
            }

            zipFile.CommitUpdate();
        }

        private void ExtractZip(string compressedFile, string destination)
        {
            using (var fileStream = File.OpenRead(compressedFile))
            {
                var zipFile = new ZipFile(fileStream);

                _logger.Debug("Validating Archive {0}", compressedFile);

                if (!zipFile.TestArchive(true, TestStrategy.FindFirstError, OnZipError))
                {
                    throw new IOException(string.Format("File {0} failed archive validation.", compressedFile));
                }

                var destinationFullPath = Path.GetFullPath(destination);

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue; // Ignore directories
                    }

                    var entryFileName = zipEntry.Name;

                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.
                    var buffer = new byte[4096]; // 4K is optimum
                    var zipStream = zipFile.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.GetFullPath(Path.Combine(destination, entryFileName));

                    // Prevent path traversal attacks - ensure extracted path is within destination
                    if (!fullZipToPath.StartsWith(destinationFullPath + Path.DirectorySeparatorChar) &&
                        !fullZipToPath.Equals(destinationFullPath, StringComparison.Ordinal))
                    {
                        _logger.Warn("Skipping zip entry with path traversal attempt: {0}", entryFileName);
                        continue;
                    }

                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
        }

        private static void ExtractTgz(string compressedFile, string destination)
        {
            Stream inStream = File.OpenRead(compressedFile);
            Stream gzipStream = new GZipInputStream(inStream);

            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, null);
            tarArchive.ExtractContents(destination);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        private void OnZipError(TestStatus status, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.Error("File {0} failed zip validation. {1}", status.File.Name, message);
            }
        }
    }
}
