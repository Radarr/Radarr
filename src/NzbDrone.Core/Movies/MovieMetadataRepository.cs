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
        bool UpsertMany(List<MovieMetadata> data);
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

        public bool UpsertMany(List<MovieMetadata> data)
        {
            var existingMetadata = FindById(data.Select(x => x.TmdbId).ToList());
            var updateMetadataList = new List<MovieMetadata>();
            var addMetadataList = new List<MovieMetadata>();
            var upToDateMetadataCount = 0;

            foreach (var meta in data)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.TmdbId == meta.TmdbId);
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

            _logger.Debug($"{upToDateMetadataCount} movie metadata up to date; Updating {updateMetadataList.Count}, Adding {addMetadataList.Count} movie metadata entries.");

            return updateMetadataList.Count > 0 || addMetadataList.Count > 0;
        }
    }
}
