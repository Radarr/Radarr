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
            var metadataFiles = _metadataFileService.GetFilesByArtist(author.Id);
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

                files.AddIfNotNull(ProcessArtistMetadata(consumer, author, consumerFiles));
                files.AddRange(ProcessArtistImages(consumer, author, consumerFiles));

                var albumGroups = bookFiles.GroupBy(s => Path.GetDirectoryName(s.Path)).ToList();

                foreach (var group in albumGroups)
                {
                    var book = _bookService.GetBook(group.First().BookId);
                    var albumFolder = group.Key;
                    files.AddIfNotNull(ProcessAlbumMetadata(consumer, author, book, albumFolder, consumerFiles));
                    files.AddRange(ProcessAlbumImages(consumer, author, book, albumFolder, consumerFiles));

                    foreach (var bookFile in group)
                    {
                        files.AddIfNotNull(ProcessTrackMetadata(consumer, author, bookFile, consumerFiles));
                    }
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Author author, BookFile bookFile)
        {
            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                files.AddIfNotNull(ProcessTrackMetadata(consumer, author, bookFile, new List<MetadataFile>()));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterBookImport(Author author, Book book, string artistFolder, string albumFolder)
        {
            var metadataFiles = _metadataFileService.GetFilesByArtist(author.Id);

            if (artistFolder.IsNullOrWhiteSpace() && albumFolder.IsNullOrWhiteSpace())
            {
                return new List<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                if (artistFolder.IsNotNullOrWhiteSpace())
                {
                    files.AddIfNotNull(ProcessArtistMetadata(consumer, author, consumerFiles));
                    files.AddRange(ProcessArtistImages(consumer, author, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Author author, List<BookFile> bookFiles)
        {
            var metadataFiles = _metadataFileService.GetFilesByArtist(author.Id);
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
                        .Where(m => m.Type == MetadataType.AlbumImage || m.Type == MetadataType.AlbumMetadata)
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

        private MetadataFile ProcessArtistMetadata(IMetadata consumer, Author author, List<MetadataFile> existingMetadataFiles)
        {
            var artistMetadata = consumer.ArtistMetadata(author);

            if (artistMetadata == null)
            {
                return null;
            }

            var hash = artistMetadata.Contents.SHA256Hash();

            var metadata = GetMetadataFile(author, existingMetadataFiles, e => e.Type == MetadataType.ArtistMetadata) ??
                               new MetadataFile
                               {
                                   AuthorId = author.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.ArtistMetadata
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

        private MetadataFile ProcessAlbumMetadata(IMetadata consumer, Author author, Book book, string albumPath, List<MetadataFile> existingMetadataFiles)
        {
            var albumMetadata = consumer.AlbumMetadata(author, book, albumPath);

            if (albumMetadata == null)
            {
                return null;
            }

            var hash = albumMetadata.Contents.SHA256Hash();

            var metadata = GetMetadataFile(author, existingMetadataFiles, e => e.Type == MetadataType.AlbumMetadata && e.BookId == book.Id) ??
                               new MetadataFile
                               {
                                   AuthorId = author.Id,
                                   BookId = book.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.AlbumMetadata
                               };

            if (hash == metadata.Hash)
            {
                if (albumMetadata.RelativePath != metadata.RelativePath)
                {
                    metadata.RelativePath = albumMetadata.RelativePath;

                    return metadata;
                }

                return null;
            }

            var fullPath = Path.Combine(author.Path, albumMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

            _logger.Debug("Writing Album Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, albumMetadata.Contents);

            metadata.Hash = hash;
            metadata.RelativePath = albumMetadata.RelativePath;
            metadata.Extension = Path.GetExtension(fullPath);

            return metadata;
        }

        private MetadataFile ProcessTrackMetadata(IMetadata consumer, Author author, BookFile bookFile, List<MetadataFile> existingMetadataFiles)
        {
            var trackMetadata = consumer.TrackMetadata(author, bookFile);

            if (trackMetadata == null)
            {
                return null;
            }

            var fullPath = Path.Combine(author.Path, trackMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

            var existingMetadata = GetMetadataFile(author, existingMetadataFiles, c => c.Type == MetadataType.TrackMetadata &&
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
                               Type = MetadataType.TrackMetadata,
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

        private List<MetadataFile> ProcessArtistImages(IMetadata consumer, Author author, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var image in consumer.ArtistImages(author))
            {
                var fullPath = Path.Combine(author.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Author image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

                var metadata = GetMetadataFile(author, existingMetadataFiles, c => c.Type == MetadataType.ArtistImage &&
                                                                              c.RelativePath == image.RelativePath) ??
                               new MetadataFile
                               {
                                   AuthorId = author.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.ArtistImage,
                                   RelativePath = image.RelativePath,
                                   Extension = Path.GetExtension(fullPath)
                               };

                DownloadImage(author, image);

                result.Add(metadata);
            }

            return result;
        }

        private List<MetadataFile> ProcessAlbumImages(IMetadata consumer, Author author, Book book, string albumFolder, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var image in consumer.AlbumImages(author, book, albumFolder))
            {
                var fullPath = Path.Combine(author.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Album image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(author, fullPath);

                var metadata = GetMetadataFile(author, existingMetadataFiles, c => c.Type == MetadataType.AlbumImage &&
                                                                                c.BookId == book.Id &&
                                                                                c.RelativePath == image.RelativePath) ??
                            new MetadataFile
                            {
                                AuthorId = author.Id,
                                BookId = book.Id,
                                Consumer = consumer.GetType().Name,
                                Type = MetadataType.AlbumImage,
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
