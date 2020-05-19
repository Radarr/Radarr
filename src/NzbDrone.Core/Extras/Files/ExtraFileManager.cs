using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Files
{
    public interface IManageExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> CreateAfterAuthorScan(Author author, List<BookFile> bookFiles);
        IEnumerable<ExtraFile> CreateAfterBookImport(Author author, BookFile bookFile);
        IEnumerable<ExtraFile> CreateAfterBookImport(Author author, Book book, string authorFolder, string bookFolder);
        IEnumerable<ExtraFile> MoveFilesAfterRename(Author author, List<BookFile> bookFiles);
        ExtraFile Import(Author author, BookFile bookFile, string path, string extension, bool readOnly);
    }

    public abstract class ExtraFileManager<TExtraFile> : IManageExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public ExtraFileManager(IConfigService configService,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService,
                                Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = logger;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> CreateAfterAuthorScan(Author author, List<BookFile> bookFiles);
        public abstract IEnumerable<ExtraFile> CreateAfterBookImport(Author author, BookFile bookFile);
        public abstract IEnumerable<ExtraFile> CreateAfterBookImport(Author author, Book book, string authorFolder, string albumFolder);
        public abstract IEnumerable<ExtraFile> MoveFilesAfterRename(Author author, List<BookFile> bookFiles);
        public abstract ExtraFile Import(Author author, BookFile bookFile, string path, string extension, bool readOnly);

        protected TExtraFile ImportFile(Author author, BookFile bookFile, string path, bool readOnly, string extension, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(bookFile.Path);
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(bookFile.Path));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extension);

            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());
            var transferMode = TransferMode.Move;

            if (readOnly)
            {
                transferMode = _configService.CopyUsingHardlinks ? TransferMode.HardLinkOrCopy : TransferMode.Copy;
            }

            _diskTransferService.TransferFile(path, newFileName, transferMode, true, false);

            return new TExtraFile
            {
                AuthorId = author.Id,
                BookId = bookFile.BookId,
                BookFileId = bookFile.Id,
                RelativePath = author.Path.GetRelativePath(newFileName),
                Extension = extension
            };
        }

        protected TExtraFile MoveFile(Author author, BookFile bookFile, TExtraFile extraFile, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(bookFile.Path);
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(bookFile.Path));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extraFile.Extension);

            var existingFileName = Path.Combine(author.Path, extraFile.RelativePath);
            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());

            if (newFileName.PathNotEquals(existingFileName))
            {
                try
                {
                    _diskProvider.MoveFile(existingFileName, newFileName);
                    extraFile.RelativePath = author.Path.GetRelativePath(newFileName);

                    return extraFile;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to move file after rename: {0}", existingFileName);
                }
            }

            return null;
        }
    }
}
