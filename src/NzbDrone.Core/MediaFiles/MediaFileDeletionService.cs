using System;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteTrackFile(Author author, BookFile bookFile);
        void DeleteTrackFile(BookFile bookFile, string subfolder = "");
    }

    public class MediaFileDeletionService : IDeleteMediaFiles,
                                            IHandleAsync<AuthorDeletedEvent>,
                                            IHandleAsync<BookDeletedEvent>,
                                            IHandle<BookFileDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAuthorService _authorService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        IAuthorService authorService,
                                        IConfigService configService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _authorService = authorService;
            _configService = configService;
            _logger = logger;
        }

        public void DeleteTrackFile(Author author, BookFile bookFile)
        {
            var fullPath = bookFile.Path;
            var rootFolder = _diskProvider.GetParentFolder(author.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Author's root folder ({0}) doesn't exist.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Author's root folder ({0}) doesn't exist.", rootFolder);
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Author's root folder ({0}) is empty.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Author's root folder ({0}) is empty.", rootFolder);
            }

            if (_diskProvider.FolderExists(author.Path))
            {
                var subfolder = _diskProvider.GetParentFolder(author.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));
                DeleteTrackFile(bookFile, subfolder);
            }
            else
            {
                // delete from db even if the author folder is missing
                _mediaFileService.Delete(bookFile, DeleteMediaFileReason.Manual);
            }
        }

        public void DeleteTrackFile(BookFile bookFile, string subfolder = "")
        {
            var fullPath = bookFile.Path;

            if (_diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting book file: {0}", fullPath);

                try
                {
                    _recycleBinProvider.DeleteFile(fullPath, subfolder);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete book file");
                    throw new NzbDroneClientException(HttpStatusCode.InternalServerError, "Unable to delete book file");
                }
            }

            // Delete the track file from the database to clean it up even if the file was already deleted
            _mediaFileService.Delete(bookFile, DeleteMediaFileReason.Manual);
        }

        public void HandleAsync(AuthorDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                var author = message.Author;
                var allArtists = _authorService.AllAuthorPaths();

                foreach (var s in allArtists)
                {
                    if (s.Key == author.Id)
                    {
                        continue;
                    }

                    if (author.Path.IsParentPath(s.Value))
                    {
                        _logger.Error("Author path: '{0}' is a parent of another author, not deleting files.", author.Path);
                        return;
                    }

                    if (author.Path.PathEquals(s.Value))
                    {
                        _logger.Error("Author path: '{0}' is the same as another author, not deleting files.", author.Path);
                        return;
                    }
                }

                if (_diskProvider.FolderExists(message.Author.Path))
                {
                    _recycleBinProvider.DeleteFolder(message.Author.Path);
                }
            }
        }

        public void HandleAsync(BookDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                var files = _mediaFileService.GetFilesByBook(message.Book.Id);
                foreach (var file in files)
                {
                    _recycleBinProvider.DeleteFile(file.Path);
                }
            }
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            if (_configService.DeleteEmptyFolders)
            {
                var author = message.BookFile.Author.Value;
                var bookFolder = message.BookFile.Path.GetParentPath();

                if (_diskProvider.GetFiles(author.Path, SearchOption.AllDirectories).Empty())
                {
                    _diskProvider.DeleteFolder(author.Path, true);
                }
                else if (_diskProvider.GetFiles(bookFolder, SearchOption.AllDirectories).Empty())
                {
                    _diskProvider.RemoveEmptySubfolders(bookFolder);
                }
            }
        }
    }
}
