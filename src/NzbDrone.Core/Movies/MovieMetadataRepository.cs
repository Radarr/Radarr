using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Translations;

namespace NzbDrone.Core.Movies
{
    public interface IMovieMetadataRepository : IBasicRepository<MovieMetadata>
    {
        MovieMetadata FindByTmdbId(int tmdbId);
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

        public MovieMetadata FindByTmdbId(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).FirstOrDefault();
        }

        public List<MovieMetadata> FindById(List<int> tmdbIds)
        {
            return Query(x => Enumerable.Contains(tmdbIds, x.TmdbId));
        }

        public List<MovieMetadata> GetMoviesWithCollections()
        {
            var movieDictionary = new Dictionary<int, MovieMetadata>();

            var builder = new SqlBuilder(_database.DatabaseType)
            .LeftJoin<MovieMetadata, MovieTranslation>((mm, t) => mm.Id == t.MovieMetadataId)
            .Where<MovieMetadata>(x => x.CollectionTmdbId > 0);

            _ = _database.QueryJoined<MovieMetadata, MovieTranslation>(
                builder,
                (metadata, translation) =>
                {
                    MovieMetadata movieEntry;

                    if (!movieDictionary.TryGetValue(metadata.Id, out movieEntry))
                    {
                        movieEntry = metadata;
                        movieDictionary.Add(movieEntry.Id, movieEntry);
                    }

                    if (translation != null)
                    {
                        movieEntry.Translations.Add(translation);
                    }
                    else
                    {
                        // Add a translation to avoid filename builder making another call thinking translations are not loaded
                        // Optimize this later by pulling translations with metadata always
                        movieEntry.Translations.Add(new MovieTranslation { Title = movieEntry.Title, Language = Language.English });
                    }

                    return movieEntry;
                });

            return movieDictionary.Values.ToList();
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
            int upToDateMetadataCount = 0;

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
