using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras
{
    public interface IExtraService
    {
        void ImportTrack(LocalBook localBook, BookFile bookFile, bool isReadOnly);
    }

    public class ExtraService : IExtraService,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<TrackFolderCreatedEvent>,
                                IHandle<AuthorRenamedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IBookService _bookService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly List<IManageExtraFiles> _extraFileManagers;
        private readonly Logger _logger;

        public ExtraService(IMediaFileService mediaFileService,
                            IBookService bookService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            List<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _bookService = bookService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public void ImportTrack(LocalBook localBook, BookFile bookFile, bool isReadOnly)
        {
            ImportExtraFiles(localBook, bookFile, isReadOnly);

            CreateAfterImport(localBook.Author, bookFile);
        }

        public void ImportExtraFiles(LocalBook localBook, BookFile bookFile, bool isReadOnly)
        {
            if (!_configService.ImportExtraFiles)
            {
                return;
            }

            var sourcePath = localBook.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var files = _diskProvider.GetFiles(sourceFolder, SearchOption.TopDirectoryOnly);

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.'))
                                                                     .ToList();

            var matchingFilenames = files.Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(sourceFileName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var filteredFilenames = new List<string>();
            var hasNfo = false;

            foreach (var matchingFilename in matchingFilenames)
            {
                // Filter out duplicate NFO files
                if (matchingFilename.EndsWith(".nfo", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (hasNfo)
                    {
                        continue;
                    }

                    hasNfo = true;
                }

                filteredFilenames.Add(matchingFilename);
            }

            foreach (var matchingFilename in filteredFilenames)
            {
                var matchingExtension = wantedExtensions.FirstOrDefault(e => matchingFilename.EndsWith(e));

                if (matchingExtension == null)
                {
                    continue;
                }

                try
                {
                    foreach (var extraFileManager in _extraFileManagers)
                    {
                        var extension = Path.GetExtension(matchingFilename);
                        var extraFile = extraFileManager.Import(localBook.Author, bookFile, matchingFilename, extension, isReadOnly);

                        if (extraFile != null)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", matchingFilename);
                }
            }
        }

        private void CreateAfterImport(Author author, BookFile bookFile)
        {
            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterTrackImport(author, bookFile);
            }
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            var author = message.Author;

            var bookFiles = GetBookFiles(author.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterAuthorScan(author, bookFiles);
            }
        }

        public void Handle(TrackFolderCreatedEvent message)
        {
            var author = message.Author;
            var book = _bookService.GetBook(message.BookFile.BookId);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterBookImport(author, book, message.AuthorFolder, message.BookFolder);
            }
        }

        public void Handle(AuthorRenamedEvent message)
        {
            var author = message.Author;
            var bookFiles = GetBookFiles(author.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(author, bookFiles);
            }
        }

        private List<BookFile> GetBookFiles(int authorId)
        {
            return _mediaFileService.GetFilesByAuthor(authorId);
        }
    }
}
