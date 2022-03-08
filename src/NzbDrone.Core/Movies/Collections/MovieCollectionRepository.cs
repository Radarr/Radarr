using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Collections
{
    public interface IMovieCollectionRepository : IBasicRepository<MovieCollection>
    {
        public MovieCollection GetByTmdbId(int tmdbId);
        bool UpsertMany(List<MovieCollection> data);
    }

    public class MovieCollectionRepository : BasicRepository<MovieCollection>, IMovieCollectionRepository
    {
        public MovieCollectionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public MovieCollection GetByTmdbId(int tmdbId)
        {
            return Query(x => x.TmdbId == tmdbId).FirstOrDefault();
        }

        public List<MovieCollection> GetByTmdbId(List<int> tmdbIds)
        {
            return Query(x => Enumerable.Contains(tmdbIds, x.TmdbId));
        }

        public bool UpsertMany(List<MovieCollection> data)
        {
            var existingMetadata = GetByTmdbId(data.Select(x => x.TmdbId).ToList());
            var updateCollectionList = new List<MovieCollection>();
            var addCollectionList = new List<MovieCollection>();
            int upToDateMetadataCount = 0;

            foreach (var collection in data)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.TmdbId == collection.TmdbId);
                if (existing != null)
                {
                    // populate Id in remote data
                    collection.Id = existing.Id;

                    // responses vary, so try adding remote to what we have
                    if (!collection.Equals(existing))
                    {
                        updateCollectionList.Add(collection);
                    }
                    else
                    {
                        upToDateMetadataCount++;
                    }
                }
                else
                {
                    addCollectionList.Add(collection);
                }
            }

            UpdateMany(updateCollectionList);
            InsertMany(addCollectionList);

            return updateCollectionList.Count > 0 || addCollectionList.Count > 0;
        }
    }
}
