using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistMetadataRepository : IBasicRepository<AuthorMetadata>
    {
        List<AuthorMetadata> FindById(List<string> foreignIds);
        bool UpsertMany(List<AuthorMetadata> data);
    }

    public class ArtistMetadataRepository : BasicRepository<AuthorMetadata>, IArtistMetadataRepository
    {
        private readonly Logger _logger;

        public ArtistMetadataRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        public List<AuthorMetadata> FindById(List<string> foreignIds)
        {
            return Query(x => Enumerable.Contains(foreignIds, x.ForeignAuthorId));
        }

        public bool UpsertMany(List<AuthorMetadata> data)
        {
            var existingMetadata = FindById(data.Select(x => x.ForeignAuthorId).ToList());
            var updateMetadataList = new List<AuthorMetadata>();
            var addMetadataList = new List<AuthorMetadata>();
            int upToDateMetadataCount = 0;

            foreach (var meta in data)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.ForeignAuthorId == meta.ForeignAuthorId);
                if (existing != null)
                {
                    // populate Id in remote data
                    meta.UseDbFieldsFrom(existing);

                    // responses vary, so try adding remote to what we have
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
