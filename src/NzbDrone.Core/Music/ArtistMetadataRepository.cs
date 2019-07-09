using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
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
        bool UpsertMany(List<ArtistMetadata> artists);
        bool UpsertMany(List<Artist> artists);
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
            artists.ForEach(x => x.ArtistMetadataId = x.Metadata.Value.Id);

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

        public bool UpsertMany(List<Artist> artists)
        {
            var result = UpsertMany(artists.Select(x => x.Metadata.Value).ToList());
            artists.ForEach(x => x.ArtistMetadataId = x.Metadata.Value.Id);
            
            return result;
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

        public bool UpsertMany(List<ArtistMetadata> data)
        {
            var existingMetadata = FindById(data.Select(x => x.ForeignArtistId).ToList());
            var updateMetadataList = new List<ArtistMetadata>();
            var addMetadataList = new List<ArtistMetadata>();
            int upToDateMetadataCount = 0;

            foreach (var meta in data)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.ForeignArtistId == meta.ForeignArtistId);
                if (existing != null)
                {
                    meta.Id = existing.Id;
                    if (!meta.Equals(existing))
                    {
                        updateMetadataList.Add(meta);
                    }
                    else
                    {
                        upToDateMetadataCount++;
                    }
                }
                else
                {
                    addMetadataList.Add(meta);
                }
            }
            
            UpdateMany(updateMetadataList);
            InsertMany(addMetadataList);
            
            _logger.Debug($"{upToDateMetadataCount} artist metadata up to date; Updating {updateMetadataList.Count}, Adding {addMetadataList.Count} artist metadata entries.");
            
            return updateMetadataList.Count > 0 || addMetadataList.Count > 0;
        }
    }
}
