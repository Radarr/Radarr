using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Metadata
{
    public class MetadataService : ExtraFileManager<MetadataFile>
    {
        private readonly IMetadataFactory _metadataFactory;
        private readonly ICleanMetadataService _cleanMetadataService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IOtherExtraFileRenamer _otherExtraFileRenamer;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpClient _httpClient;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IBookService _bookService;
        private readonly Logger _logger;

        public MetadataService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               IRecycleBinProvider recycleBinProvider,
                               IOtherExtraFileRenamer otherExtraFileRenamer,
                               IMetadataFactory metadataFactory,
                               ICleanMetadataService cleanMetadataService,
                               IHttpClient httpClient,
                               IMediaFileAttributeService mediaFileAttributeService,
                               IMetadataFileService metadataFileService,
                               IBookService bookService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _metadataFactory = metadataFactory;
            _cleanMetadataService = cleanMetadataService;
            _otherExtraFileRenamer = otherExtraFileRenamer;
            _recycleBinProvider = recycleBinProvider;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _httpClient = httpClient;
            _mediaFileAttributeService = mediaFileAttributeService;
            _metadataFileService = metadataFileService;
            _bookService = bookService;
            _logger = logger;
        }

        public override int Order => 0;

        public override IEnumerable<ExtraFile> CreateAfterAuthorScan(Author author, List<BookFile> bookFiles)
        {
            var metadataFiles = _metadataFileService.GetFilesByAuthor(author.Id);
            _cleanMetadataService.Clean(author);

            if (!_diskProvider.FolderExists(author.Path))
            {
                _logger.Info("Author folder does not exist, skipping metadata creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                files.AddIfNotNull(ProcessAuthorMetadata(consumer, author, consumerFiles));
                files.AddRange(ProcessAuthorImages(consumer, author, consumerFiles));

                foreach (var bookFile in bookFiles)
                {
                    files.AddIfNotNull(ProcessBookMetadata(consumer, author, bookFile, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterBookImport(Author author, BookFile bookFile)
        {
            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                files.AddIfNotNull(ProcessBookMetadata(consumer, author, bookFile, new List<MetadataFile>()));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterBookImport(Author author, Book book, string authorFolder, string albumFolder)
        {
            var metadataFiles = _metadataFileService.GetFilesByAuthor(author.Id);

            if (authorFolder.IsNullOrWhiteSpace() && albumFolder.IsNullOrWhiteSpace())
            {
                return new List<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                if (authorFolder.IsNotNullOrWhiteSpace())
                {
                    files.AddIfNotNull(ProcessAuthorMetadata(consumer, author, consumerFiles));
                    files.AddRange(ProcessAuthorImages(consumer, author, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Author author, List<BookFile> bookFiles)
        {
            var metadataFiles = _metadataFileService.GetFilesByAuthor(author.Id);
            var movedFiles = new List<MetadataFile>();
            var distinctTrackFilePaths = bookFiles.DistinctBy(s => Path.GetDirectoryName(s.Path)).ToList();

            // TODO: Move EpisodeImage and EpisodeMetadata metadata files, instead of relying on consumers to do it
            // (Xbmc's EpisodeImage is more than just the extension)
            foreach (var consumer in _metadataFactory.GetAvailableProviders())
            {
                foreach (var filePath in distinctTrackFilePaths)
                {
                    var metadataFilesForConsumer = GetMetadataFilesForConsumer(consumer, metadataFiles)
                        .Where(m => m.BookId == filePath.BookId)
                        .Where(m => m.Type == MetadataType.BookImage || m.Type == MetadataType.BookMetadata)
                        .ToList();

                    foreach (var metadataFile in metadataFilesForConsumer)
                    {
                        var newFileName = consumer.GetFilenameAfterMove(author, Path.GetDirectoryName(filePath.Path), metadataFile);
                        var existingFileName = Path.Combine(author.Path, metadataFile.RelativePath);

                        if (newFileName.PathNotEquals(existingFileName))
                        {
                            try
                            {
                                _diskProvider.MoveFile(existingFileName, newFileName);
                                metadataFile.RelativePath = author.Path.GetRelativePath(newFileName);
                                movedFiles.Add(metadataFile);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to move metadata file after rename: {0}", existingFileName);
                            }
                        }
                    }
                }

                foreach (var bookFile in bookFiles)
                {
                    var metadataFilesForConsumer = GetMetadataFilesForConsumer(consumer, metadataFiles).Where(m => m.BookFileId == bookFile.Id).ToList();

                    foreach (var metadataFile in metadataFilesForConsumer)
                    {
                        var newFileName = consumer.GetFilenameAfterMove(author, bookFile, metadataFile);
                        var existingFileName = Path.Combine(author.Path, metadataFile.RelativePath);

                        if (newFileName.PathNotEquals(existingFileName))
                        {
                            try
                            {
                                _diskProvider.MoveFile(existingFileName, newFileName);
                                metadataFile.RelativePath = author.Path.GetRelativePath(newFileName);
                                movedFiles.Add(metadataFile);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to move metadata file after rename: {0}", existingFileName);
                            }
                        }
                    }
                }
            }

            _metadataFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Author author, BookFile bookFile, string path, string extension, bool readOnly)
        {
            return null;
        }

        private List<MetadataFile> GetMetadataFilesForConsumer(IMetadata consumer, List<MetadataFile> artistMetadata)
        {
            return artistMetadata.Where(c => c.Consumer == consumer.GetType().Name).ToList();
        }

        private MetadataFile ProcessAuthorMetadata(IMetadata consumer, Author author, List<MetadataFile> existingMetadataFiles)
        {
            var artistMetadata = consumer.AuthorMetadata(author);

            if (artistMetadata == null)
            {
                return null;
            }

            var hash = artistMetadata.Contents.SHA256Hash();

            var metadata = GetMetadataFile(author, existingMetadataFiles, e => e.Type == MetadataType.AuthorMetadata) ??
                               new MetadataFile
                               {
                                   AuthorId = author.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.AuthorMetadata
                               };

            if (hash == metadata.Hash)
            {
                if (artistMetadata.RelativePath != metadata.RelativePath)
                {
                    metadata.RelativePath = artistMetadata.RelativePath;

                    return metadata;
                }

                return null;
            }

            var fullPath = Path.Combine(author.Path, artistMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

            _logger.Debug("Writing Author Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, artistMetadata.Contents);

            metadata.Hash = hash;
            metadata.RelativePath = artistMetadata.RelativePath;
            metadata.Extension = Path.GetExtension(fullPath);

            return metadata;
        }

        private MetadataFile ProcessBookMetadata(IMetadata consumer, Author author, BookFile bookFile, List<MetadataFile> existingMetadataFiles)
        {
            var trackMetadata = consumer.BookMetadata(author, bookFile);

            if (trackMetadata == null)
            {
                return null;
            }

            var fullPath = Path.Combine(author.Path, trackMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

            var existingMetadata = GetMetadataFile(author, existingMetadataFiles, c => c.Type == MetadataType.BookMetadata &&
                                                                                  c.BookFileId == bookFile.Id);

            if (existingMetadata != null)
            {
                var existingFullPath = Path.Combine(author.Path, existingMetadata.RelativePath);
                if (fullPath.PathNotEquals(existingFullPath))
                {
                    _diskTransferService.TransferFile(existingFullPath, fullPath, TransferMode.Move);
                    existingMetadata.RelativePath = trackMetadata.RelativePath;
                }
            }

            var hash = trackMetadata.Contents.SHA256Hash();

            var metadata = existingMetadata ??
                           new MetadataFile
                           {
                               AuthorId = author.Id,
                               BookId = bookFile.BookId,
                               BookFileId = bookFile.Id,
                               Consumer = consumer.GetType().Name,
                               Type = MetadataType.BookMetadata,
                               RelativePath = trackMetadata.RelativePath,
                               Extension = Path.GetExtension(fullPath)
                           };

            if (hash == metadata.Hash)
            {
                return null;
            }

            _logger.Debug("Writing Track Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, trackMetadata.Contents);

            metadata.Hash = hash;

            return metadata;
        }

        private List<MetadataFile> ProcessAuthorImages(IMetadata consumer, Author author, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var image in consumer.AuthorImages(author))
            {
                var fullPath = Path.Combine(author.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Author image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

                var metadata = GetMetadataFile(author, existingMetadataFiles, c => c.Type == MetadataType.AuthorImage &&
                                                                              c.RelativePath == image.RelativePath) ??
                               new MetadataFile
                               {
                                   AuthorId = author.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.AuthorImage,
                                   RelativePath = image.RelativePath,
                                   Extension = Path.GetExtension(fullPath)
                               };

                DownloadImage(author, image);

                result.Add(metadata);
            }

            return result;
        }

        private void DownloadImage(Author author, ImageFileResult image)
        {
            var fullPath = Path.Combine(author.Path, image.RelativePath);

            try
            {
                if (image.Url.StartsWith("http"))
                {
                    _httpClient.DownloadFile(image.Url, fullPath);
                }
                else
                {
                    _diskProvider.CopyFile(image.Url, fullPath);
                }

                _mediaFileAttributeService.SetFilePermissions(fullPath);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex, "Couldn't download image {0} for {1}. {2}", image.Url, author, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't download image {0} for {1}", image.Url, author);
            }
        }

        private void SaveMetadataFile(string path, string contents)
        {
            _diskProvider.WriteAllText(path, contents);
            _mediaFileAttributeService.SetFilePermissions(path);
        }

        private MetadataFile GetMetadataFile(Author author, List<MetadataFile> existingMetadataFiles, Func<MetadataFile, bool> predicate)
        {
            var matchingMetadataFiles = existingMetadataFiles.Where(predicate).ToList();

            if (matchingMetadataFiles.Empty())
            {
                return null;
            }

            //Remove duplicate metadata files from DB and disk
            foreach (var file in matchingMetadataFiles.Skip(1))
            {
                var path = Path.Combine(author.Path, file.RelativePath);

                _logger.Debug("Removing duplicate Metadata file: {0}", path);

                var subfolder = _diskProvider.GetParentFolder(author.Path).GetRelativePath(_diskProvider.GetParentFolder(path));
                _recycleBinProvider.DeleteFile(path, subfolder);
                _metadataFileService.Delete(file.Id);
            }

            return matchingMetadataFiles.First();
        }
    }
}
