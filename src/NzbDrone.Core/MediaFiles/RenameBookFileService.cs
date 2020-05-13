using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRenameTrackFileService
    {
        List<RenameBookFilePreview> GetRenamePreviews(int authorId);
        List<RenameBookFilePreview> GetRenamePreviews(int authorId, int bookId);
    }

    public class RenameBookFileService : IRenameTrackFileService, IExecute<RenameFilesCommand>, IExecute<RenameAuthorCommand>
    {
        private readonly IAuthorService _authorService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveBookFiles _bookFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RenameBookFileService(IAuthorService authorService,
                                        IMediaFileService mediaFileService,
                                        IMoveBookFiles bookFileMover,
                                        IEventAggregator eventAggregator,
                                        IBuildFileNames filenameBuilder,
                                        IDiskProvider diskProvider,
                                        Logger logger)
        {
            _authorService = authorService;
            _mediaFileService = mediaFileService;
            _bookFileMover = bookFileMover;
            _eventAggregator = eventAggregator;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameBookFilePreview> GetRenamePreviews(int authorId)
        {
            var author = _authorService.GetAuthor(authorId);
            var files = _mediaFileService.GetFilesByAuthor(authorId);

            _logger.Trace($"got {files.Count} files");

            return GetPreviews(author, files)
                .OrderByDescending(e => e.BookId)
                .ToList();
        }

        public List<RenameBookFilePreview> GetRenamePreviews(int authorId, int bookId)
        {
            var author = _authorService.GetAuthor(authorId);
            var files = _mediaFileService.GetFilesByBook(bookId);

            return GetPreviews(author, files)
                .OrderByDescending(e => e.TrackNumbers.First()).ToList();
        }

        private IEnumerable<RenameBookFilePreview> GetPreviews(Author author, List<BookFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var book = file.Book.Value;
                var bookFilePath = file.Path;

                if (book == null)
                {
                    _logger.Warn("File ({0}) is not linked to a book", bookFilePath);
                    continue;
                }

                var newName = _filenameBuilder.BuildBookFileName(author, book, file);

                _logger.Trace($"got name {newName}");

                var newPath = _filenameBuilder.BuildBookFilePath(author, book, newName, Path.GetExtension(bookFilePath));

                _logger.Trace($"got path {newPath}");

                if (!bookFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameBookFilePreview
                    {
                        AuthorId = author.Id,
                        BookId = book.Id,
                        BookFileId = file.Id,
                        ExistingPath = file.Path,
                        NewPath = newPath
                    };
                }
            }
        }

        private void RenameFiles(List<BookFile> bookFiles, Author author)
        {
            var renamed = new List<BookFile>();

            foreach (var bookFile in bookFiles)
            {
                var bookFilePath = bookFile.Path;

                try
                {
                    _logger.Debug("Renaming book file: {0}", bookFile);
                    _bookFileMover.MoveBookFile(bookFile, author);

                    _mediaFileService.Update(bookFile);
                    renamed.Add(bookFile);

                    _logger.Debug("Renamed book file: {0}", bookFile);

                    _eventAggregator.PublishEvent(new BookFileRenamedEvent(author, bookFile, bookFilePath));
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to rename file {0}", bookFilePath);
                }
            }

            if (renamed.Any())
            {
                _eventAggregator.PublishEvent(new AuthorRenamedEvent(author));

                _logger.Debug("Removing Empty Subfolders from: {0}", author.Path);
                _diskProvider.RemoveEmptySubfolders(author.Path);
            }
        }

        public void Execute(RenameFilesCommand message)
        {
            var author = _authorService.GetAuthor(message.AuthorId);
            var bookFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", bookFiles.Count, author.Name);
            RenameFiles(bookFiles, author);
            _logger.ProgressInfo("Selected book files renamed for {0}", author.Name);
        }

        public void Execute(RenameAuthorCommand message)
        {
            _logger.Debug("Renaming all files for selected author");
            var artistToRename = _authorService.GetAuthors(message.AuthorIds);

            foreach (var author in artistToRename)
            {
                var bookFiles = _mediaFileService.GetFilesByAuthor(author.Id);
                _logger.ProgressInfo("Renaming all files in author: {0}", author.Name);
                RenameFiles(bookFiles, author);
                _logger.ProgressInfo("All book files renamed for {0}", author.Name);
            }
        }
    }
}
