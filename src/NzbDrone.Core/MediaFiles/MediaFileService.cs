using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        BookFile Add(BookFile bookFile);
        void AddMany(List<BookFile> bookFiles);
        void Update(BookFile bookFile);
        void Update(List<BookFile> bookFiles);
        void Delete(BookFile bookFile, DeleteMediaFileReason reason);
        void DeleteMany(List<BookFile> bookFiles, DeleteMediaFileReason reason);
        List<BookFile> GetFilesByAuthor(int authorId);
        List<BookFile> GetFilesByBook(int bookId);
        List<BookFile> GetUnmappedFiles();
        List<IFileInfo> FilterUnchangedFiles(List<IFileInfo> files, FilterFilesType filter);
        BookFile Get(int id);
        List<BookFile> Get(IEnumerable<int> ids);
        List<BookFile> GetFilesWithBasePath(string path);
        List<BookFile> GetFileWithPath(List<string> path);
        BookFile GetFileWithPath(string path);
        void UpdateMediaInfo(List<BookFile> bookFiles);
    }

    public class MediaFileService : IMediaFileService,
        IHandle<AuthorMovedEvent>,
        IHandleAsync<BookDeletedEvent>,
        IHandleAsync<ModelEvent<RootFolder>>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IEventAggregator eventAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public BookFile Add(BookFile bookFile)
        {
            var addedFile = _mediaFileRepository.Insert(bookFile);
            _eventAggregator.PublishEvent(new BookFileAddedEvent(addedFile));
            return addedFile;
        }

        public void AddMany(List<BookFile> bookFiles)
        {
            _mediaFileRepository.InsertMany(bookFiles);
            foreach (var addedFile in bookFiles)
            {
                _eventAggregator.PublishEvent(new BookFileAddedEvent(addedFile));
            }
        }

        public void Update(BookFile bookFile)
        {
            _mediaFileRepository.Update(bookFile);
        }

        public void Update(List<BookFile> bookFiles)
        {
            _mediaFileRepository.UpdateMany(bookFiles);
        }

        public void Delete(BookFile bookFile, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.Delete(bookFile);

            // If the trackfile wasn't mapped to a track, don't publish an event
            if (bookFile.BookId > 0)
            {
                _eventAggregator.PublishEvent(new BookFileDeletedEvent(bookFile, reason));
            }
        }

        public void DeleteMany(List<BookFile> bookFiles, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.DeleteMany(bookFiles);

            // publish events where trackfile was mapped to a track
            foreach (var bookFile in bookFiles.Where(x => x.BookId > 0))
            {
                _eventAggregator.PublishEvent(new BookFileDeletedEvent(bookFile, reason));
            }
        }

        public List<IFileInfo> FilterUnchangedFiles(List<IFileInfo> files, FilterFilesType filter)
        {
            if (filter == FilterFilesType.None)
            {
                return files;
            }

            _logger.Debug($"Filtering {files.Count} files for unchanged files");

            var knownFiles = GetFileWithPath(files.Select(x => x.FullName).ToList());
            _logger.Trace($"Got {knownFiles.Count} existing files");

            if (!knownFiles.Any())
            {
                return files;
            }

            var combined = files
                .Join(knownFiles,
                      f => f.FullName,
                      af => af.Path,
                      (f, af) => new { DiskFile = f, DbFile = af },
                      PathEqualityComparer.Instance)
                .ToList();
            _logger.Trace($"Matched paths for {combined.Count} files");

            List<IFileInfo> unwanted = null;
            if (filter == FilterFilesType.Known)
            {
                unwanted = combined
                    .Where(x => x.DiskFile.Length == x.DbFile.Size &&
                           Math.Abs((x.DiskFile.LastWriteTimeUtc - x.DbFile.Modified).TotalSeconds) <= 1)
                    .Select(x => x.DiskFile)
                    .ToList();
                _logger.Trace($"{unwanted.Count} unchanged existing files");
            }
            else if (filter == FilterFilesType.Matched)
            {
                unwanted = combined
                    .Where(x => x.DiskFile.Length == x.DbFile.Size &&
                           Math.Abs((x.DiskFile.LastWriteTimeUtc - x.DbFile.Modified).TotalSeconds) <= 1 &&
                           (x.DbFile.Book == null || (x.DbFile.Book.IsLoaded && x.DbFile.Book.Value != null)))
                    .Select(x => x.DiskFile)
                    .ToList();
                _logger.Trace($"{unwanted.Count} unchanged and matched files");
            }
            else
            {
                throw new ArgumentException("Unrecognised value of FilterFilesType filter");
            }

            return files.Except(unwanted).ToList();
        }

        public BookFile Get(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<BookFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public List<BookFile> GetFilesWithBasePath(string path)
        {
            return _mediaFileRepository.GetFilesWithBasePath(path);
        }

        public List<BookFile> GetFileWithPath(List<string> path)
        {
            return _mediaFileRepository.GetFileWithPath(path);
        }

        public BookFile GetFileWithPath(string path)
        {
            return _mediaFileRepository.GetFileWithPath(path);
        }

        public List<BookFile> GetFilesByAuthor(int authorId)
        {
            return _mediaFileRepository.GetFilesByAuthor(authorId);
        }

        public List<BookFile> GetFilesByBook(int bookId)
        {
            return _mediaFileRepository.GetFilesByBook(bookId);
        }

        public List<BookFile> GetUnmappedFiles()
        {
            return _mediaFileRepository.GetUnmappedFiles();
        }

        public void UpdateMediaInfo(List<BookFile> bookFiles)
        {
            _mediaFileRepository.SetFields(bookFiles, t => t.MediaInfo);
        }

        public void Handle(AuthorMovedEvent message)
        {
            var files = _mediaFileRepository.GetFilesWithBasePath(message.SourcePath);

            foreach (var file in files)
            {
                var newPath = message.DestinationPath + file.Path.Substring(message.SourcePath.Length);
                file.Path = newPath;
            }

            Update(files);
        }

        public void HandleAsync(BookDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                _mediaFileRepository.DeleteFilesByBook(message.Book.Id);
            }
            else
            {
                _mediaFileRepository.UnlinkFilesByBook(message.Book.Id);
            }
        }

        public void HandleAsync(ModelEvent<RootFolder> message)
        {
            if (message.Action == ModelAction.Deleted)
            {
                var files = GetFilesWithBasePath(message.Model.Path);
                DeleteMany(files, DeleteMediaFileReason.Manual);
            }
        }
    }
}
