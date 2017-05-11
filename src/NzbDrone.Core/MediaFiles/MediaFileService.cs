using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;
using NzbDrone.Core.Music;
using System;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        TrackFile Add(TrackFile trackFile);
        void Update(TrackFile trackFile);
        void Delete(TrackFile trackFile, DeleteMediaFileReason reason);
        List<TrackFile> GetFilesByArtist(string artistId);
        List<TrackFile> GetFilesWithoutMediaInfo();
        List<string> FilterExistingFiles(List<string> files, Artist artist);
        TrackFile Get(int id);
        List<TrackFile> Get(IEnumerable<int> ids);

    }

    public class MediaFileService : IMediaFileService, IHandleAsync<ArtistDeletedEvent>
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

        public void Update(TrackFile trackFile)
        {
            _mediaFileRepository.Update(trackFile);
        }

        public void Delete(TrackFile trackFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the tracks and artist attached for the event consumers
            trackFile.Episodes.LazyLoad();
            trackFile.Path = Path.Combine(trackFile.Artist.Value.Path, trackFile.RelativePath);

            _mediaFileRepository.Delete(trackFile);
            _eventAggregator.PublishEvent(new TrackFileDeletedEvent(trackFile, reason));
        }


        public List<TrackFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Artist artist)
        {
            var artistFiles = GetFilesByArtist(artist.SpotifyId).Select(f => Path.Combine(artist.Path, f.RelativePath)).ToList();

            if (!artistFiles.Any()) return files;

            return files.Except(artistFiles, PathEqualityComparer.Instance).ToList();
        }

        public TrackFile Get(int id)
        {
            // TODO: Should this be spotifyID or DB Id?
            return _mediaFileRepository.Get(id);
        }

        public List<TrackFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            var files = GetFilesByArtist(message.Artist.SpotifyId);
            _mediaFileRepository.DeleteMany(files);
        }

        public List<TrackFile> GetFilesByArtist(string artistId)
        {
            return _mediaFileRepository.GetFilesByArtist(artistId);
        }
    }
}