using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService
    {
        Album GetAlbum(int albumId);
        List<Album> GetAlbums(IEnumerable<int> albumIds);
        PagingSpec<Album> Paged(PagingSpec<Album> pagingSpec);
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

    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public AlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        public Album GetAlbum(int albumId)
        {
            return _albumRepository.Get(albumId);
        }

        public List<Album> GetAlbums(IEnumerable<int> albumIds)
        {
            return _albumRepository.Get(albumIds).ToList();
        }

        public PagingSpec<Album> Paged(PagingSpec<Album> pagingSpec)
        {
            return _albumRepository.GetPaged(pagingSpec);
        }

        public Album AddAlbum(Album newAlbum)
        {
            newAlbum.Added = DateTime.UtcNow;
            return _albumRepository.Insert(newAlbum);
        }

        public List<Album> AddAlbums(List<Album> newAlbums)
        {
            var now = DateTime.UtcNow;
            foreach (var album in newAlbums)
            {
                album.Added = now;
            }

            _albumRepository.InsertMany(newAlbums);
            return newAlbums;
        }

        public Album FindByForeignId(string foreignAlbumId)
        {
            return _albumRepository.FindByForeignId(foreignAlbumId);
        }

        public Album FindByPath(string path)
        {
            return _albumRepository.FindByPath(path);
        }

        public List<Album> FindByArtistId(int artistId)
        {
            return _albumRepository.FindByArtistId(artistId);
        }

        public Dictionary<int, string> AllAlbumPaths()
        {
            return _albumRepository.AllAlbumPaths();
        }

        public List<Album> GetAlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            return _albumRepository.AlbumsBetweenDates(start, end, includeUnmonitored);
        }

        public void DeleteAlbum(int albumId, bool deleteFiles)
        {
            _albumRepository.Delete(albumId);
        }

        public void DeleteAlbums(List<int> albumIds, bool deleteFiles)
        {
            _albumRepository.DeleteMany(albumIds);
        }

        public List<Album> GetAllAlbums()
        {
            return _albumRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllAlbumTags()
        {
            return _albumRepository.AllAlbumTags();
        }

        public Album UpdateAlbum(Album album)
        {
            return _albumRepository.Update(album);
        }

        public List<Album> UpdateAlbums(List<Album> albums)
        {
            _albumRepository.UpdateMany(albums);
            return albums;
        }

        public bool AlbumPathExists(string folder)
        {
            return _albumRepository.AlbumPathExists(folder);
        }
    }
}
