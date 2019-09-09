using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NLog;

namespace NzbDrone.Core.Music
{
    public interface IArtistMetadataRepository : IBasicRepository<ArtistMetadata>
    {
        List<ArtistMetadata> FindById(List<string> foreignIds);
        bool UpsertMany(List<ArtistMetadata> artists);
    }

    public class ArtistMetadataRepository : BasicRepository<ArtistMetadata>, IArtistMetadataRepository
    {
        private readonly Logger _logger;

        public ArtistMetadataRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
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
                    meta.UseDbFieldsFrom(existing);
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
