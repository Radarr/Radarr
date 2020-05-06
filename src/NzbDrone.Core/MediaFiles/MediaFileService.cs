using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        BookFile Add(BookFile trackFile);
        void AddMany(List<BookFile> trackFiles);
        void Update(BookFile trackFile);
        void Update(List<BookFile> trackFile);
        void Delete(BookFile trackFile, DeleteMediaFileReason reason);
        void DeleteMany(List<BookFile> trackFiles, DeleteMediaFileReason reason);
        List<BookFile> GetFilesByArtist(int authorId);
        List<BookFile> GetFilesByAlbum(int bookId);
        List<BookFile> GetUnmappedFiles();
        List<IFileInfo> FilterUnchangedFiles(List<IFileInfo> files, FilterFilesType filter);
        BookFile Get(int id);
        List<BookFile> Get(IEnumerable<int> ids);
        List<BookFile> GetFilesWithBasePath(string path);
        List<BookFile> GetFileWithPath(List<string> path);
        BookFile GetFileWithPath(string path);
        void UpdateMediaInfo(List<BookFile> trackFiles);
    }

    public class MediaFileService : IMediaFileService,
        IHandle<ArtistMovedEvent>,
        IHandleAsync<AlbumDeletedEvent>,
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

        public BookFile Add(BookFile trackFile)
        {
            var addedFile = _mediaFileRepository.Insert(trackFile);
            _eventAggregator.PublishEvent(new TrackFileAddedEvent(addedFile));
            return addedFile;
        }

        public void AddMany(List<BookFile> trackFiles)
        {
            _mediaFileRepository.InsertMany(trackFiles);
            foreach (var addedFile in trackFiles)
            {
                _eventAggregator.PublishEvent(new TrackFileAddedEvent(addedFile));
            }
        }

        public void Update(BookFile trackFile)
        {
            _mediaFileRepository.Update(trackFile);
        }

        public void Update(List<BookFile> trackFiles)
        {
            _mediaFileRepository.UpdateMany(trackFiles);
        }

        public void Delete(BookFile trackFile, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.Delete(trackFile);

            // If the trackfile wasn't mapped to a track, don't publish an event
            if (trackFile.BookId > 0)
            {
                _eventAggregator.PublishEvent(new TrackFileDeletedEvent(trackFile, reason));
            }
        }

        public void DeleteMany(List<BookFile> trackFiles, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.DeleteMany(trackFiles);

            // publish events where trackfile was mapped to a track
            foreach (var trackFile in trackFiles.Where(x => x.BookId > 0))
            {
                _eventAggregator.PublishEvent(new TrackFileDeletedEvent(trackFile, reason));
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
                           (x.DbFile.Album == null || (x.DbFile.Album.IsLoaded && x.DbFile.Album.Value != null)))
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

        public List<BookFile> GetFilesByArtist(int authorId)
        {
            return _mediaFileRepository.GetFilesByArtist(authorId);
        }

        public List<BookFile> GetFilesByAlbum(int bookId)
        {
            return _mediaFileRepository.GetFilesByAlbum(bookId);
        }

        public List<BookFile> GetUnmappedFiles()
        {
            return _mediaFileRepository.GetUnmappedFiles();
        }

        public void UpdateMediaInfo(List<BookFile> trackFiles)
        {
            _mediaFileRepository.SetFields(trackFiles, t => t.MediaInfo);
        }

        public void Handle(ArtistMovedEvent message)
        {
            var files = _mediaFileRepository.GetFilesWithBasePath(message.SourcePath);

            foreach (var file in files)
            {
                var newPath = message.DestinationPath + file.Path.Substring(message.SourcePath.Length);
                file.Path = newPath;
            }

            Update(files);
        }

        public void HandleAsync(AlbumDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                _mediaFileRepository.DeleteFilesByAlbum(message.Album.Id);
            }
            else
            {
                _mediaFileRepository.UnlinkFilesByAlbum(message.Album.Id);
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
