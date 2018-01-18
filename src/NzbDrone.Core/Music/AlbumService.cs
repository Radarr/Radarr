using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;
using System.Text;
using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService
    {
        Album GetAlbum(int albumId);
        List<Album> GetAlbums(IEnumerable<int> albumIds);
        List<Album> GetAlbumsByArtist(int artistId);
        Album AddAlbum(Album newAlbum);
        List<Album> AddAlbums(List<Album> newAlbums);
        Album FindById(string spotifyId);
        Album FindByTitle(int artistId, string title);
        Album FindByTitleInexact(string title);
        void DeleteAlbum(int albumId, bool deleteFiles);
        List<Album> GetAllAlbums();
        Album UpdateAlbum(Album album);
        List<Album> UpdateAlbums(List<Album> album);
        void SetAlbumMonitored(int albumId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        void InsertMany(List<Album> albums);
        void UpdateMany(List<Album> albums);
        void DeleteMany(List<Album> albums);
        void RemoveAddOptions(Album album);
    }

    public class AlbumService : IAlbumService,
                                IHandleAsync<ArtistDeletedEvent>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackService _trackService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly Logger _logger;

        public AlbumService(IAlbumRepository albumRepository,
                            IEventAggregator eventAggregator,
                            ITrackService trackService,
                            IBuildFileNames fileNameBuilder,
                            Logger logger)
        {
            _albumRepository = albumRepository;
            _eventAggregator = eventAggregator;
            _trackService = trackService;
            _fileNameBuilder = fileNameBuilder;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            _albumRepository.Insert(newAlbum);
            //_eventAggregator.PublishEvent(new AlbumAddedEvent(GetAlbum(newAlbum.Id)));

            return newAlbum;
        }

        public List<Album> AddAlbums(List<Album> newAlbums)
        {
            _albumRepository.InsertMany(newAlbums);
            //_eventAggregator.PublishEvent(new AlbumsAddedEvent(newAlbums.Select(s => s.Id).ToList()));

            return newAlbums;
        }

        public void DeleteAlbum(int albumId, bool deleteFiles)
        {
            var album = _albumRepository.Get(albumId);
            _albumRepository.Delete(albumId);
            _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, deleteFiles));
        }

        public Album FindById(string lidarrId)
        {
            return _albumRepository.FindById(lidarrId);
        }

        public Album FindByTitle(int artistId, string title)
        {
            return _albumRepository.FindByTitle(artistId, title);
        }

        public Album FindByTitleInexact(string title)
        {
            throw new NotImplementedException();
        }

        public List<Album> GetAllAlbums()
        {
            return _albumRepository.All().ToList();
        }

        public Album GetAlbum(int albumId)
        {
            return _albumRepository.Get(albumId);
        }

        public List<Album> GetAlbums(IEnumerable<int> albumIds)
        {
            return _albumRepository.Get(albumIds).ToList();
        }

        public List<Album> GetAlbumsByArtist(int artistId)
        {
            return _albumRepository.GetAlbums(artistId).ToList();
        }

        public void RemoveAddOptions(Album album)
        {
            _albumRepository.SetFields(album, s => s.AddOptions);
        }

        public PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec)
        {
            var albumResult = _albumRepository.AlbumsWithoutFiles(pagingSpec);

            return albumResult;
        }

        public List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var albums = _albumRepository.AlbumsBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return albums;
        }

        public void InsertMany(List<Album> albums)
        {
            _albumRepository.InsertMany(albums);
        }

        public void UpdateMany(List<Album> albums)
        {
            _albumRepository.UpdateMany(albums);
        }

        public void DeleteMany(List<Album> albums)
        {
            _albumRepository.DeleteMany(albums);

            foreach (var album in albums)
            {
                _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, false));
            }
        }

        public Album UpdateAlbum(Album album)
        {
            var storedAlbum = GetAlbum(album.Id);

            var updatedAlbum = _albumRepository.Update(album);
            _eventAggregator.PublishEvent(new AlbumEditedEvent(updatedAlbum, storedAlbum));

            return updatedAlbum;
        }

        public void SetAlbumMonitored(int albumId, bool monitored)
        {
            var album = _albumRepository.Get(albumId);
            _albumRepository.SetMonitoredFlat(album, monitored);

            var tracks = _trackService.GetTracksByAlbum(albumId);
            foreach (var track in tracks)
            {
                track.Monitored = monitored;
            }
            _trackService.UpdateTracks(tracks);

            _logger.Debug("Monitored flag for Album:{0} was set to {1}", albumId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _albumRepository.SetMonitored(ids, monitored);
            foreach (var id in ids)
            {
                var tracks = _trackService.GetTracksByAlbum(id);
                foreach (var track in tracks)
                {
                    track.Monitored = monitored;
                }
                _trackService.UpdateTracks(tracks);
            }
        }

        public List<Album> UpdateAlbums(List<Album> album)
        {
            _logger.Debug("Updating {0} album", album.Count);

            _albumRepository.UpdateMany(album);
            _logger.Debug("{0} albums updated", album.Count);

            return album;
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            var albums = GetAlbumsByArtist(message.Artist.Id);
            _albumRepository.DeleteMany(albums);
        }
    }
}
