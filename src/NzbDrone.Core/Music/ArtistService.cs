using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser;
using System.Text;
using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IArtistService
    {
        Artist GetArtist(int artistId);
        List<Artist> GetArtists(IEnumerable<int> artistIds);
        Artist AddArtist(Artist newArtist);
        Artist FindById(string spotifyId);
        Artist FindByName(string title);
        Artist FindByTitleInexact(string title);
        void DeleteArtist(int artistId, bool deleteFiles);
        List<Artist> GetAllArtists();
        List<Artist> AllForTag(int tagId);
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

        public ArtistService(IArtistRepository artistRepository,
                            IEventAggregator eventAggregator,
                            ITrackService trackService,
                            IBuildFileNames fileNameBuilder,
                            Logger logger)
        {
            _artistRepository = artistRepository;
            _eventAggregator = eventAggregator;
            _trackService = trackService;
            _fileNameBuilder = fileNameBuilder;
            _logger = logger;
        }

        public Artist AddArtist(Artist newArtist)
        {
            _artistRepository.Insert(newArtist);
            _eventAggregator.PublishEvent(new ArtistAddedEvent(GetArtist(newArtist.Id)));

            return newArtist;
        }

        public bool ArtistPathExists(string folder)
        {
            return _artistRepository.ArtistPathExists(folder);
        }

        public void DeleteArtist(int artistId, bool deleteFiles)
        {
            var artist = _artistRepository.Get(artistId);
            _artistRepository.Delete(artistId);
            _eventAggregator.PublishEvent(new ArtistDeletedEvent(artist, deleteFiles));
        }

        public Artist FindById(string spotifyId)
        {
            return _artistRepository.FindById(spotifyId);
        }

        public Artist FindByName(string title)
        {
            return _artistRepository.FindByName(title.CleanArtistName());
        }

        public Artist FindByTitleInexact(string title)
        {
            throw new NotImplementedException();
        }

        public List<Artist> GetAllArtists()
        {
            return _artistRepository.All().ToList();
        }

        public List<Artist> AllForTag(int tagId)
        {
            return GetAllArtists().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        public Artist GetArtist(int artistDBId)
        {
            return _artistRepository.Get(artistDBId);
        }


        public List<Artist> GetArtists(IEnumerable<int> artistIds)
        {
            return _artistRepository.Get(artistIds).ToList();
        }

        public void RemoveAddOptions(Artist artist)
        {
            _artistRepository.SetFields(artist, s => s.AddOptions);
        }

        public Artist UpdateArtist(Artist artist)
        {
            var storedArtist = GetArtist(artist.Id); // Is it Id or iTunesId? 

            foreach (var album in artist.Albums)
            {
                var storedAlbum = storedArtist.Albums.SingleOrDefault(s => s.ForeignAlbumId == album.ForeignAlbumId);

                if (storedAlbum != null && album.Monitored != storedAlbum.Monitored)
                {
                    _trackService.SetTrackMonitoredByAlbum(artist.Id, album.Id, album.Monitored);
                }
            }

            var updatedArtist = _artistRepository.Update(artist);
            _eventAggregator.PublishEvent(new ArtistEditedEvent(updatedArtist, storedArtist));

            return updatedArtist;
        }

        public List<Artist> UpdateArtists(List<Artist> artist)
        {
            _logger.Debug("Updating {0} artist", artist.Count);
            foreach (var s in artist)
            {
                _logger.Trace("Updating: {0}", s.Name);
                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    // Build the artist folder name instead of using the existing folder name.
                    // This may lead to folder name changes, but consistent with adding a new artist.

                    s.Path = Path.Combine(s.RootFolderPath, _fileNameBuilder.GetArtistFolder(s));

                    _logger.Trace("Changing path for {0} to {1}", s.Name, s.Path);
                }

                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Name);
                }
            }

            _artistRepository.UpdateMany(artist);
            _logger.Debug("{0} artists updated", artist.Count);

            return artist;
        }
    }
}
