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
        Artist GetArtistByMetadataId(int artistMetadataId);
        List<Artist> GetArtists(IEnumerable<int> artistIds);
        Artist AddArtist(Artist newArtist);
        List<Artist> AddArtists(List<Artist> newArtists);
        Artist FindById(string spotifyId);
        Artist FindByName(string title);
        Artist FindByNameInexact(string title);
        void DeleteArtist(int artistId, bool deleteFiles);
        List<Artist> GetAllArtists();
        List<Artist> AllForTag(int tagId);
        Artist UpdateArtist(Artist artist);
        List<Artist> UpdateArtists(List<Artist> artist, bool useExistingRelativeFolder);
        bool ArtistPathExists(string folder);
        void RemoveAddOptions(Artist artist);
    }

    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackService _trackService;
        private readonly IBuildArtistPaths _artistPathBuilder;
        private readonly Logger _logger;

        public ArtistService(IArtistRepository artistRepository,
                             IArtistMetadataRepository artistMetadataRepository,
                             IEventAggregator eventAggregator,
                             ITrackService trackService,
                             IBuildArtistPaths artistPathBuilder,
                             Logger logger)
        {
            _artistRepository = artistRepository;
            _artistMetadataRepository = artistMetadataRepository;
            _eventAggregator = eventAggregator;
            _trackService = trackService;
            _artistPathBuilder = artistPathBuilder;
            _logger = logger;
        }

        public Artist AddArtist(Artist newArtist)
        {
            _artistMetadataRepository.Upsert(newArtist);
            _artistRepository.Insert(newArtist);
            _eventAggregator.PublishEvent(new ArtistAddedEvent(GetArtist(newArtist.Id)));

            return newArtist;
        }

        public List<Artist> AddArtists(List<Artist> newArtists)
        {
            _artistMetadataRepository.UpsertMany(newArtists);
            _artistRepository.InsertMany(newArtists);
            _eventAggregator.PublishEvent(new ArtistsImportedEvent(newArtists.Select(s => s.Id).ToList()));

            return newArtists;
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

        public Artist FindByNameInexact(string title)
        {
            const double fuzzThreshold = 0.8;
            const double fuzzGap = 0.2;
            var cleanTitle = Parser.Parser.CleanArtistName(title);

            if (string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = title;
            }

            var sortedArtists = GetAllArtists()
                .Select(s => new
                    {
                        MatchProb = s.CleanName.FuzzyMatch(cleanTitle),
                        Artist = s
                    })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            if (!sortedArtists.Any())
            {
                return null;
            }

            _logger.Trace("\nFuzzy artist match on '{0}':\n{1}",
                          cleanTitle,
                          string.Join("\n", sortedArtists.Select(x => $"{x.Artist.CleanName}: {x.MatchProb}")));

            if (sortedArtists[0].MatchProb > fuzzThreshold
                && (sortedArtists.Count == 1 || sortedArtists[0].MatchProb - sortedArtists[1].MatchProb > fuzzGap))
            {
                return sortedArtists[0].Artist;
            }

            return null;
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

        public Artist GetArtistByMetadataId(int artistMetadataId)
        {
            return _artistRepository.GetArtistByMetadataId(artistMetadataId);
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
            var updatedArtist = _artistMetadataRepository.Update(artist);
            updatedArtist = _artistRepository.Update(updatedArtist);
            _eventAggregator.PublishEvent(new ArtistEditedEvent(updatedArtist, storedArtist));

            return updatedArtist;
        }

        public List<Artist> UpdateArtists(List<Artist> artist, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} artist", artist.Count);

            foreach (var s in artist)
            {
                _logger.Trace("Updating: {0}", s.Name);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _artistPathBuilder.BuildPath(s, useExistingRelativeFolder);

                    //s.Path = Path.Combine(s.RootFolderPath, _fileNameBuilder.GetArtistFolder(s));

                    _logger.Trace("Changing path for {0} to {1}", s.Name, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Name);
                }
            }

            _artistMetadataRepository.UpdateMany(artist);
            _artistRepository.UpdateMany(artist);
            _logger.Debug("{0} artists updated", artist.Count);

            return artist;
        }
    }
}
