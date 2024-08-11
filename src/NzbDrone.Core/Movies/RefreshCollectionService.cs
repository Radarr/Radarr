using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class RefreshCollectionService : IExecute<RefreshCollectionsCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieCollectionService _collectionService;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IAddMovieService _addMovieService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Logger _logger;

        public RefreshCollectionService(IProvideMovieInfo movieInfo,
                                        IMovieCollectionService collectionService,
                                        IMovieService movieService,
                                        IMovieMetadataService movieMetadataService,
                                        IAddMovieService addMovieService,
                                        IImportListExclusionService importListExclusionService,
                                        IEventAggregator eventAggregator,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _collectionService = collectionService;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _addMovieService = addMovieService;
            _importListExclusionService = importListExclusionService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private MovieCollection RefreshCollectionInfo(int collectionId)
        {
            // Get the movie before updating, that way any changes made to the movie after the refresh started,
            // but before this movie was refreshed won't be lost.
            var collection = _collectionService.GetCollection(collectionId);

            _logger.ProgressInfo("Updating info for {0}", collection.Title);

            MovieCollection collectionInfo;
            List<MovieMetadata> movies;

            try
            {
                collectionInfo = _movieInfo.GetCollectionInfo(collection.TmdbId);
            }
            catch (MovieNotFoundException)
            {
                _collectionService.RemoveCollection(collection);
                _logger.Debug("Removing collection not present on TMDb for {0}", collection.Title);

                throw;
            }

            collection.Title = collectionInfo.Title;
            collection.Overview = collectionInfo.Overview;
            collection.CleanTitle = collectionInfo.CleanTitle;
            collection.SortTitle = collectionInfo.SortTitle;
            collection.LastInfoSync = DateTime.UtcNow;
            collection.Images = collectionInfo.Images;

            movies = collectionInfo.Movies;
            movies.ForEach(x => x.CollectionTmdbId = collection.TmdbId);

            var existingMetaForCollection = _movieMetadataService.GetMoviesByCollectionTmdbId(collection.TmdbId);

            var updateList = new List<MovieMetadata>();

            foreach (var remoteMovie in movies)
            {
                var existing = existingMetaForCollection.FirstOrDefault(e => e.TmdbId == remoteMovie.TmdbId);

                if (existingMetaForCollection.Any(x => x.TmdbId == remoteMovie.TmdbId))
                {
                    existingMetaForCollection.Remove(existing);
                }

                updateList.Add(remoteMovie);
            }

            _movieMetadataService.UpsertMany(updateList);
            _movieMetadataService.DeleteMany(existingMetaForCollection);

            _logger.Debug("Finished collection refresh for {0}", collection.Title);

            _collectionService.UpdateCollection(collection);

            return collection;
        }

        public bool ShouldRefresh(MovieCollection collection)
        {
            if (collection.LastInfoSync == null || collection.LastInfoSync < DateTime.UtcNow.AddDays(-15))
            {
                _logger.Trace("Collection {0} last updated more than 15 days ago, should refresh.", collection.Title);
                return true;
            }

            if (collection.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
            {
                _logger.Trace("Collection {0} last updated less than 6 hours ago, should not be refreshed.", collection.Title);
                return false;
            }

            return false;
        }

        private void SyncCollectionMovies(MovieCollection collection)
        {
            if (collection.Monitored)
            {
                var existingMovies = _movieService.AllMovieTmdbIds();
                var collectionMovies = _movieMetadataService.GetMoviesByCollectionTmdbId(collection.TmdbId);
                var excludedMovies = _importListExclusionService.All().Select(e => e.TmdbId);
                var moviesToAdd = collectionMovies.Where(m => !existingMovies.Contains(m.TmdbId)).Where(m => !excludedMovies.Contains(m.TmdbId));

                if (moviesToAdd.Any())
                {
                    _addMovieService.AddMovies(moviesToAdd.Select(m => new Movie
                    {
                        TmdbId = m.TmdbId,
                        Title = m.Title,
                        QualityProfileId = collection.QualityProfileId,
                        RootFolderPath = collection.RootFolderPath,
                        MinimumAvailability = collection.MinimumAvailability,
                        AddOptions = new AddMovieOptions
                        {
                            SearchForMovie = collection.SearchOnAdd,
                            AddMethod = AddMovieMethod.Collection
                        },
                        Monitored = true,
                        Tags = collection.Tags
                    }).ToList(), true);
                }
            }
        }

        public void Execute(RefreshCollectionsCommand message)
        {
            if (message.CollectionIds.Any())
            {
                foreach (var collectionId in message.CollectionIds)
                {
                    var newCollection = RefreshCollectionInfo(collectionId);
                    SyncCollectionMovies(newCollection);
                }
            }
            else
            {
                var allCollections = _collectionService.GetAllCollections().OrderBy(c => c.SortTitle).ToList();

                foreach (var collection in allCollections)
                {
                    try
                    {
                        var newCollection = collection;

                        if (ShouldRefresh(collection) || message.Trigger == CommandTrigger.Manual)
                        {
                            newCollection = RefreshCollectionInfo(collection.Id);
                        }

                        SyncCollectionMovies(newCollection);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Collection '{0}' (TMDb {1}) was not found, it may have been removed from The Movie Database.", collection.Title, collection.TmdbId);
                    }
                }
            }

            _eventAggregator.PublishEvent(new CollectionRefreshCompleteEvent());
        }
    }
}
