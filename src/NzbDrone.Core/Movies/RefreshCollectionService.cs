using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class RefreshCollectionService : IExecute<RefreshCollectionsCommand>, IHandle<CollectionEditedEvent>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieCollectionService _collectionService;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IAddMovieService _addMovieService;

        private readonly Logger _logger;

        public RefreshCollectionService(IProvideMovieInfo movieInfo,
                                        IMovieCollectionService collectionService,
                                        IMovieService movieService,
                                        IMovieMetadataService movieMetadataService,
                                        IAddMovieService addMovieService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _collectionService = collectionService;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _addMovieService = addMovieService;
            _logger = logger;
        }

        private MovieCollection RefreshCollectionInfo(int collectionId)
        {
            // Get the movie before updating, that way any changes made to the movie after the refresh started,
            // but before this movie was refreshed won't be lost.
            var collection = _collectionService.GetCollection(collectionId);

            _logger.ProgressInfo("Updating info for {0}", collection.Title);

            MovieCollection collectionInfo;

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

            collectionInfo.Movies.ForEach(x => x.CollectionTmdbId = collection.TmdbId);
            _movieMetadataService.UpsertMany(collectionInfo.Movies);

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

                _addMovieService.AddMovies(collectionMovies.Where(m => !existingMovies.Contains(m.TmdbId)).Select(m => new Movie
                {
                    TmdbId = m.TmdbId,
                    Title = m.Title,
                    ProfileId = collection.QualityProfileId,
                    RootFolderPath = collection.RootFolderPath,
                    MinimumAvailability = collection.MinimumAvailability,
                    AddOptions = new AddMovieOptions
                    {
                        SearchForMovie = collection.SearchOnAdd
                    },
                    Monitored = true
                }).ToList());
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
        }

        public void Handle(CollectionEditedEvent message)
        {
            SyncCollectionMovies(message.Collection);
        }
    }
}
