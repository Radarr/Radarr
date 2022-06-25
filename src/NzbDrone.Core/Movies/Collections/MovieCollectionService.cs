using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Collections
{
    public interface IMovieCollectionService
    {
        MovieCollection AddCollection(MovieCollection collection);
        MovieCollection GetCollection(int id);
        MovieCollection FindByTmdbId(int tmdbId);
        IEnumerable<MovieCollection> GetCollections(IEnumerable<int> ids);
        List<MovieCollection> GetAllCollections();
        MovieCollection UpdateCollection(MovieCollection collection);
        List<MovieCollection> UpdateCollections(List<MovieCollection> collections);
        void RemoveCollection(MovieCollection collection);
        bool Upsert(MovieCollection collection);
        bool UpsertMany(List<MovieCollection> collections);
    }

    public class MovieCollectionService : IMovieCollectionService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMovieCollectionRepository _repo;
        private readonly IMovieService _movieService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MovieCollectionService(IMovieCollectionRepository repo, IMovieService movieService, IEventAggregator eventAggregator, Logger logger)
        {
            _repo = repo;
            _movieService = movieService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public MovieCollection AddCollection(MovieCollection newCollection)
        {
            var existing = _repo.GetByTmdbId(newCollection.TmdbId);

            if (existing == null)
            {
                var collection = _repo.Insert(newCollection);

                _eventAggregator.PublishEvent(new CollectionAddedEvent(collection));

                return collection;
            }

            return existing;
        }

        public MovieCollection GetCollection(int id)
        {
            return _repo.Get(id);
        }

        public IEnumerable<MovieCollection> GetCollections(IEnumerable<int> ids)
        {
            return _repo.Get(ids);
        }

        public List<MovieCollection> GetAllCollections()
        {
            return _repo.All().ToList();
        }

        public MovieCollection UpdateCollection(MovieCollection collection)
        {
            var storedCollection = GetCollection(collection.Id);

            var updatedCollection =  _repo.Update(collection);

            _eventAggregator.PublishEvent(new CollectionEditedEvent(updatedCollection, storedCollection));

            return updatedCollection;
        }

        public List<MovieCollection> UpdateCollections(List<MovieCollection> collections)
        {
            _logger.Debug("Updating {0} movie collections", collections.Count);

            foreach (var c in collections)
            {
                _logger.Trace("Updating: {0}", c.Title);
            }

            _repo.UpdateMany(collections);
            _logger.Debug("{0} movie collections updated", collections.Count);

            return collections;
        }

        public void RemoveCollection(MovieCollection collection)
        {
            _repo.Delete(collection);

            _eventAggregator.PublishEvent(new CollectionDeletedEvent(collection));
        }

        public bool Upsert(MovieCollection collection)
        {
            return _repo.UpsertMany(new List<MovieCollection> { collection });
        }

        public bool UpsertMany(List<MovieCollection> collections)
        {
            return _repo.UpsertMany(collections);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            var collections = message.Movies.Select(x => x.MovieMetadata.Value.CollectionTmdbId).Distinct();

            foreach (var collectionTmdbId in collections)
            {
                if (collectionTmdbId == 0 || _movieService.GetMoviesByCollectionTmdbId(collectionTmdbId).Any())
                {
                    continue;
                }

                var collection = FindByTmdbId(collectionTmdbId);

                _repo.Delete(collectionTmdbId);

                _eventAggregator.PublishEvent(new CollectionDeletedEvent(collection));
            }
        }

        public MovieCollection FindByTmdbId(int tmdbId)
        {
            return _repo.GetByTmdbId(tmdbId);
        }
    }
}
