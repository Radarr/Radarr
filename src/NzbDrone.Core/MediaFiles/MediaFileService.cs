using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Common;
using NzbDrone.Core.Music;
using System;
using NzbDrone.Core.Music.Events;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        TrackFile Add(TrackFile trackFile);
        void AddMany(List<TrackFile> trackFiles);
        void Update(TrackFile trackFile);
        void Update(List<TrackFile> trackFile);
        void Delete(TrackFile trackFile, DeleteMediaFileReason reason);
        void DeleteMany(List<TrackFile> trackFiles, DeleteMediaFileReason reason);
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesByRelease(int releaseId);
        List<TrackFile> GetUnmappedFiles();
        List<IFileInfo> FilterUnchangedFiles(List<IFileInfo> files, Artist artist, FilterFilesType filter);
        TrackFile Get(int id);
        List<TrackFile> Get(IEnumerable<int> ids);
        List<TrackFile> GetFilesWithBasePath(string path);
        TrackFile GetFileWithPath(string path);
        void UpdateMediaInfo(List<TrackFile> trackFiles);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<AlbumDeletedEvent>
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

        public TrackFile Add(TrackFile trackFile)
        {
            var addedFile = _mediaFileRepository.Insert(trackFile);
            _eventAggregator.PublishEvent(new TrackFileAddedEvent(addedFile));
            return addedFile;
        }

        public void AddMany(List<TrackFile> trackFiles)
        {
            _mediaFileRepository.InsertMany(trackFiles);
            foreach (var addedFile in trackFiles)
            {
                _eventAggregator.PublishEvent(new TrackFileAddedEvent(addedFile));
            }
        }

        public void Update(TrackFile trackFile)
        {
            _mediaFileRepository.Update(trackFile);
        }

        public void Update(List<TrackFile> trackFiles)
        {
            _mediaFileRepository.UpdateMany(trackFiles);
        }


        public void Delete(TrackFile trackFile, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.Delete(trackFile);
            // If the trackfile wasn't mapped to a track, don't publish an event
            if (trackFile.AlbumId > 0)
            {
                _eventAggregator.PublishEvent(new TrackFileDeletedEvent(trackFile, reason));
            }
        }

        public void DeleteMany(List<TrackFile> trackFiles, DeleteMediaFileReason reason)
        {
            _mediaFileRepository.DeleteMany(trackFiles);

            // publish events where trackfile was mapped to a track
            foreach (var trackFile in trackFiles.Where(x => x.AlbumId > 0))
            {
                _eventAggregator.PublishEvent(new TrackFileDeletedEvent(trackFile, reason));
            }
        }

        public List<IFileInfo> FilterUnchangedFiles(List<IFileInfo> files, Artist artist, FilterFilesType filter)
        {
            _logger.Debug($"Filtering {files.Count} files for unchanged files");

            var knownFiles = GetFilesWithBasePath(artist.Path);
            _logger.Trace($"Got {knownFiles.Count} existing files");

            if (!knownFiles.Any()) return files;

            var combined = files
                .Join(knownFiles,
                      f => f.FullName,
                      af => af.Path,
                      (f, af) => new { DiskFile = f, DbFile = af},
                      PathEqualityComparer.Instance)
                .ToList();

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
                           (x.DbFile.Tracks == null || (x.DbFile.Tracks.IsLoaded && x.DbFile.Tracks.Value.Any())))
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

        public TrackFile Get(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<TrackFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public List<TrackFile> GetFilesWithBasePath(string path)
        {
            return _mediaFileRepository.GetFilesWithBasePath(path);
        }

        public TrackFile GetFileWithPath(string path)
        {
            return _mediaFileRepository.GetFileWithPath(path);
        }

        public void HandleAsync(AlbumDeletedEvent message)
        {
            _mediaFileRepository.DeleteFilesByAlbum(message.Album.Id);
        }

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            return _mediaFileRepository.GetFilesByArtist(artistId);
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return _mediaFileRepository.GetFilesByAlbum(albumId);
        }

        public List<TrackFile> GetFilesByRelease(int releaseId)
        {
            return _mediaFileRepository.GetFilesByRelease(releaseId);
        }

        public List<TrackFile> GetUnmappedFiles()
        {
            return _mediaFileRepository.GetUnmappedFiles();
        }

        public void UpdateMediaInfo(List<TrackFile> trackFiles)
        {
            _mediaFileRepository.SetFields(trackFiles, t => t.MediaInfo);
        }
    }
}
