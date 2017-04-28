using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface IArtistService
    {
        Artist GetArtist(int artistId);
        List<Artist> GetArtists(IEnumerable<int> artistIds);
        Artist AddArtist(Artist newArtist);
        Artist FindByItunesId(int itunesId);
        Artist FindByTitle(string title);
        Artist FindByTitleInexact(string title);
        void DeleteArtist(int artistId, bool deleteFiles);
        List<Artist> GetAllArtists();
        Artist UpdateArtist(Artist artist);
        List<Artist> UpdateArtists(List<Artist> artist);
        bool ArtistPathExists(string folder);
        void RemoveAddOptions(Artist artist);
    }

    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackService _trackService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly Logger _logger;

        public Artist AddArtist(Artist newArtist)
        {
            throw new NotImplementedException();
        }

        public bool ArtistPathExists(string folder)
        {
            throw new NotImplementedException();
        }

        public void DeleteArtist(int artistId, bool deleteFiles)
        {
            throw new NotImplementedException();
        }

        public Artist FindByItunesId(int itunesId)
        {
            throw new NotImplementedException();
        }

        public Artist FindByTitle(string title)
        {
            throw new NotImplementedException();
        }

        public Artist FindByTitleInexact(string title)
        {
            throw new NotImplementedException();
        }

        public List<Artist> GetAllArtists()
        {
            throw new NotImplementedException();
        }

        public Artist GetArtist(int artistId)
        {
            throw new NotImplementedException();
        }

        public List<Artist> GetArtists(IEnumerable<int> artistIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveAddOptions(Artist artist)
        {
            throw new NotImplementedException();
        }

        public Artist UpdateArtist(Artist artist)
        {
            throw new NotImplementedException();
        }

        public List<Artist> UpdateArtists(List<Artist> artist)
        {
            throw new NotImplementedException();
        }
    }
}
