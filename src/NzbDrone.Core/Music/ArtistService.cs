using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaItems;

namespace NzbDrone.Core.Music
{
    public interface IArtistService : IBaseMediaService<Artist>
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

    public class ArtistService : BaseMediaService<Artist>, IArtistService
    {
        private readonly IArtistRepository _artistRepository;

        public ArtistService(IArtistRepository artistRepository)
        {
            _artistRepository = artistRepository;
        }

        protected override IBasicRepository<Artist> Repository => _artistRepository;

        public Artist GetArtist(int artistId) => Get(artistId);
        public List<Artist> GetArtists(IEnumerable<int> artistIds) => Get(artistIds);
        public Artist AddArtist(Artist newArtist) => Add(newArtist);
        public List<Artist> AddArtists(List<Artist> newArtists) => AddMany(newArtists);
        public void DeleteArtist(int artistId, bool deleteFiles) => Delete(artistId, deleteFiles);
        public void DeleteArtists(List<int> artistIds, bool deleteFiles) => DeleteMany(artistIds, deleteFiles);
        public List<Artist> GetAllArtists() => GetAll();
        public Artist UpdateArtist(Artist artist) => Update(artist);
        public List<Artist> UpdateArtists(List<Artist> artists) => UpdateMany(artists);

        public Artist FindByName(string name) => _artistRepository.FindByName(name);
        public Artist FindByForeignId(string foreignArtistId) => _artistRepository.FindByForeignId(foreignArtistId);
        public List<Artist> GetMonitoredArtists() => _artistRepository.GetMonitored();
        public bool ArtistPathExists(string path) => _artistRepository.ArtistPathExists(path);
    }
}
