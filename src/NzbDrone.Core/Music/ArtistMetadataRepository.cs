using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using Marr.Data;
using NLog;

namespace NzbDrone.Core.Music
{
    public interface IArtistMetadataRepository : IBasicRepository<ArtistMetadata>
    {
        Artist Insert(Artist artist);
        List<Artist> InsertMany(List<Artist> artists);
        Artist Update(Artist artist);
        Artist Upsert(Artist artist);
        void UpdateMany(List<Artist> artists);
        ArtistMetadata FindById(string foreignArtistId);
        List<ArtistMetadata> FindById(List<string> foreignIds);
        void UpsertMany(List<ArtistMetadata> artists);
        void UpsertMany(List<Artist> artists);
    }

    public class ArtistMetadataRepository : BasicRepository<ArtistMetadata>, IArtistMetadataRepository
    {
        private readonly Logger _logger;

        public ArtistMetadataRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        public Artist Insert(Artist artist)
        {
            Insert(artist.Metadata.Value);
            artist.ArtistMetadataId = artist.Metadata.Value.Id;
            return artist;
        }

        public List<Artist> InsertMany(List<Artist> artists)
        {
            InsertMany(artists.Select(x => x.Metadata.Value).ToList());
            foreach (var artist in artists)
            {
                artist.ArtistMetadataId = artist.Metadata.Value.Id;
            }
            return artists;
        }

        public Artist Update(Artist artist)
        {
            Update(artist.Metadata.Value);
            return artist;
        }

        public Artist Upsert(Artist artist)
        {
            var existing = FindById(artist.Metadata.Value.ForeignArtistId);
            if (existing != null)
            {
                artist.ArtistMetadataId = existing.Id;
                artist.Metadata.Value.Id = existing.Id;
                Update(artist);
            }
            else
            {
                Insert(artist);
            }
            _logger.Debug("Upserted metadata with ID {0}", artist.Id);
            return artist;
        }

        public void UpsertMany(List<Artist> artists)
        {
            foreach (var artist in artists)
            {
                Upsert(artist);
            }
        }

        public void UpdateMany(List<Artist> artists)
        {
            UpdateMany(artists.Select(x => x.Metadata.Value).ToList());
        }

        public ArtistMetadata FindById(string foreignArtistId)
        {
            return Query.Where(a => a.ForeignArtistId == foreignArtistId).SingleOrDefault();
        }

        public List<ArtistMetadata> FindById(List<string> foreignIds)
        {
            return Query.Where($"[ForeignArtistId] IN ('{string.Join("','", foreignIds)}')").ToList();
        }

        public void UpsertMany(List<ArtistMetadata> artists)
        {
            foreach (var artist in artists)
            {
                var existing = FindById(artist.ForeignArtistId);
                if (existing != null)
                {
                    artist.Id = existing.Id;
                    Update(artist);
                }
                else
                {
                    Insert(artist);
                }
                _logger.Debug("Upserted metadata with ID {0}", artist.Id);
            }
        }
    }
}
