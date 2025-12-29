using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaItems;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService : IBaseMediaService<Album>
    {
        Album GetAlbum(int albumId);
        List<Album> GetAlbums(IEnumerable<int> albumIds);
        Album AddAlbum(Album newAlbum);
        List<Album> AddAlbums(List<Album> newAlbums);
        Album FindByForeignId(string foreignAlbumId);
        Album FindByPath(string path);
        List<Album> FindByArtistId(int artistId);
        Dictionary<int, string> AllAlbumPaths();
        List<Album> GetAlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        void DeleteAlbum(int albumId, bool deleteFiles);
        void DeleteAlbums(List<int> albumIds, bool deleteFiles);
        List<Album> GetAllAlbums();
        Dictionary<int, List<int>> AllAlbumTags();
        Album UpdateAlbum(Album album);
        List<Album> UpdateAlbums(List<Album> albums);
        bool AlbumPathExists(string folder);
    }

    public class AlbumService : BaseMediaService<Album>, IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public AlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        protected override IBasicRepository<Album> Repository => _albumRepository;

        public Album GetAlbum(int albumId) => Get(albumId);
        public List<Album> GetAlbums(IEnumerable<int> albumIds) => Get(albumIds);
        public Album AddAlbum(Album newAlbum) => Add(newAlbum);
        public List<Album> AddAlbums(List<Album> newAlbums) => AddMany(newAlbums);
        public void DeleteAlbum(int albumId, bool deleteFiles) => Delete(albumId, deleteFiles);
        public void DeleteAlbums(List<int> albumIds, bool deleteFiles) => DeleteMany(albumIds, deleteFiles);
        public List<Album> GetAllAlbums() => GetAll();
        public Album UpdateAlbum(Album album) => Update(album);
        public List<Album> UpdateAlbums(List<Album> albums) => UpdateMany(albums);

        public Album FindByForeignId(string foreignAlbumId) => _albumRepository.FindByForeignId(foreignAlbumId);
        public Album FindByPath(string path) => _albumRepository.FindByPath(path);
        public List<Album> FindByArtistId(int artistId) => _albumRepository.FindByArtistId(artistId);
        public Dictionary<int, string> AllAlbumPaths() => _albumRepository.AllAlbumPaths();
        public List<Album> GetAlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
            => _albumRepository.AlbumsBetweenDates(start, end, includeUnmonitored);
        public Dictionary<int, List<int>> AllAlbumTags() => _albumRepository.AllAlbumTags();
        public bool AlbumPathExists(string folder) => _albumRepository.AlbumPathExists(folder);
    }
}
