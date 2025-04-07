using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataRepository : IBasicRepository<MovieMetadata>
    {
        MovieMetadata FindByTmdbId(int tmdbId);
        MovieMetadata FindByImdbId(string imdbId);
        List<MovieMetadata> FindById(List<int> tmdbIds);
        List<MovieMetadata> GetMoviesWithCollections();
        List<MovieMetadata> GetMoviesByCollectionTmdbId(int collectionId);
        bool UpsertMany(List<MovieMetadata> metadatas);
    }

    public class MovieMetadataRepository : BasicRepository<MovieMetadata>, IMovieMetadataRepository
    {
        private readonly Logger _logger;

        public MovieMetadataRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        public MovieMetadata FindByTmdbId(int tmdbId)
        {
            return Query(x => x.TmdbId == tmdbId).FirstOrDefault();
        }

        public MovieMetadata FindByImdbId(string imdbId)
        {
            return Query(x => x.ImdbId == imdbId).FirstOrDefault();
        }

        public List<MovieMetadata> FindById(List<int> tmdbIds)
        {
            return Query(x => Enumerable.Contains(tmdbIds, x.TmdbId));
        }

        public List<MovieMetadata> GetMoviesWithCollections()
        {
            return Query(x => x.CollectionTmdbId > 0);
        }

        public List<MovieMetadata> GetMoviesByCollectionTmdbId(int collectionId)
        {
            return Query(x => x.CollectionTmdbId == collectionId);
        }

        public bool UpsertMany(List<MovieMetadata> metadatas)
        {
            var updateList = new List<MovieMetadata>();
            var addList = new List<MovieMetadata>();
            var upToDateCount = 0;

            var existingMetadatas = FindById(metadatas.Select(x => x.TmdbId).ToList());

            foreach (var metadata in metadatas)
            {
                var existingMetadata = existingMetadatas.SingleOrDefault(x => x.TmdbId == metadata.TmdbId);

                if (existingMetadata != null)
                {
                    metadata.UseDbFieldsFrom(existingMetadata);

                    if (!metadata.Equals(existingMetadata))
                    {
                        updateList.Add(metadata);
                    }
                    else
                    {
                        upToDateCount++;
                    }
                }
                else
                {
                    addList.Add(metadata);
                }
            }

            UpdateMany(updateList);
            InsertMany(addList);

            _logger.Debug("{0} movie metadata up to date; Updating {1}, Adding {2} entries.", upToDateCount, updateList.Count, addList.Count);

            return updateList.Count > 0 || addList.Count > 0;
        }
    }
}
