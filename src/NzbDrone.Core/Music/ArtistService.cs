using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public interface IArtistService
    {
        Artist GetArtist(int artistId);
        List<Artist> GetArtists(IEnumerable<int> artistIds);
        Artist AddArtist(Artist newArtist);
        List<Artist> AddArtists(List<Artist> newArtists);
        Artist FindByName(string name);
        Artist FindByForeignId(string foreignArtistId);
        void DeleteArtist(int artistId, bool deleteFiles);
        void DeleteArtists(List<int> artistIds, bool deleteFiles);
        List<Artist> GetAllArtists();
        List<Artist> GetMonitoredArtists();
        Artist UpdateArtist(Artist artist);
        List<Artist> UpdateArtists(List<Artist> artists);
        bool ArtistPathExists(string path);
    }

    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;

        public ArtistService(IArtistRepository artistRepository)
        {
            _artistRepository = artistRepository;
        }

        public Artist GetArtist(int artistId)
        {
            return _artistRepository.Get(artistId);
        }

        public List<Artist> GetArtists(IEnumerable<int> artistIds)
        {
            return _artistRepository.Get(artistIds).ToList();
        }

        public Artist AddArtist(Artist newArtist)
        {
            newArtist.Added = DateTime.UtcNow;
            return _artistRepository.Insert(newArtist);
        }

        public List<Artist> AddArtists(List<Artist> newArtists)
        {
            var now = DateTime.UtcNow;
            foreach (var artist in newArtists)
            {
                artist.Added = now;
            }

            _artistRepository.InsertMany(newArtists);
            return newArtists;
        }

        public Artist FindByName(string name)
        {
            return _artistRepository.FindByName(name);
        }

        public Artist FindByForeignId(string foreignArtistId)
        {
            return _artistRepository.FindByForeignId(foreignArtistId);
        }

        public void DeleteArtist(int artistId, bool deleteFiles)
        {
            _artistRepository.Delete(artistId);
        }

        public void DeleteArtists(List<int> artistIds, bool deleteFiles)
        {
            _artistRepository.DeleteMany(artistIds);
        }

        public List<Artist> GetAllArtists()
        {
            return _artistRepository.All().ToList();
        }

        public List<Artist> GetMonitoredArtists()
        {
            return _artistRepository.GetMonitored();
        }

        public Artist UpdateArtist(Artist artist)
        {
            return _artistRepository.Update(artist);
        }

        public List<Artist> UpdateArtists(List<Artist> artists)
        {
            _artistRepository.UpdateMany(artists);
            return artists;
        }

        public bool ArtistPathExists(string path)
        {
            return _artistRepository.ArtistPathExists(path);
        }
    }
}
