using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Collections
{
    [V3ApiController]
    public class CollectionController : RestControllerWithSignalR<CollectionResource, MovieCollection>,
                                        IHandle<CollectionAddedEvent>,
                                        IHandle<CollectionEditedEvent>,
                                        IHandle<CollectionDeletedEvent>
    {
        private readonly IMovieCollectionService _collectionService;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly INamingConfigService _namingService;
        private readonly IManageCommandQueue _commandQueueManager;

        public CollectionController(IBroadcastSignalRMessage signalRBroadcaster,
                                    IMovieCollectionService collectionService,
                                    IMovieService movieService,
                                    IMovieMetadataService movieMetadataService,
                                    IBuildFileNames fileNameBuilder,
                                    INamingConfigService namingService,
                                    IManageCommandQueue commandQueueManager)
            : base(signalRBroadcaster)
        {
            _collectionService = collectionService;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
            _commandQueueManager = commandQueueManager;
        }

        protected override CollectionResource GetResourceById(int id)
        {
            return MapToResource(_collectionService.GetCollection(id));
        }

        [HttpGet]
        public List<CollectionResource> GetCollections()
        {
            var collectionMovies = _movieMetadataService.GetMoviesWithCollections();

            return MapToResource(_collectionService.GetAllCollections(), collectionMovies).ToList();
        }

        [RestPutById]
        public ActionResult<CollectionResource> UpdateCollection(CollectionResource collectionResource)
        {
            var collection = _collectionService.GetCollection(collectionResource.Id);

            var model = collectionResource.ToModel(collection);

            var updatedMovie = _collectionService.UpdateCollection(model);

            return Accepted(updatedMovie.Id);
        }

        [HttpPut]
        public ActionResult UpdateCollections(CollectionUpdateResource resource)
        {
            var collectionsToUpdate = _collectionService.GetCollections(resource.CollectionIds);

            foreach (var collection in collectionsToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    collection.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    collection.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.MinimumAvailability.HasValue)
                {
                    collection.MinimumAvailability = resource.MinimumAvailability.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    collection.RootFolderPath = resource.RootFolderPath;
                }

                if (resource.MonitorMovies.HasValue)
                {
                    var movies = _movieService.GetMoviesByCollectionTmdbId(collection.TmdbId);

                    movies.ForEach(c => c.Monitored = resource.MonitorMovies.Value);

                    _movieService.UpdateMovie(movies, true);
                }
            }

            var updated = _collectionService.UpdateCollections(collectionsToUpdate.ToList()).ToResource();

            _commandQueueManager.Push(new RefreshCollectionsCommand());

            return Accepted(updated);
        }

        private IEnumerable<CollectionResource> MapToResource(List<MovieCollection> collections, List<MovieMetadata> collectionMovies)
        {
            // Avoid calling for naming spec on every movie in filenamebuilder
            var namingConfig = _namingService.GetConfig();

            foreach (var collection in collections)
            {
                var resource = collection.ToResource();

                foreach (var movie in collectionMovies.Where(m => m.CollectionTmdbId == collection.TmdbId))
                {
                    var movieResource = movie.ToResource();
                    movieResource.Folder = _fileNameBuilder.GetMovieFolder(new Movie { MovieMetadata = movie }, namingConfig);

                    resource.Movies.Add(movieResource);
                }

                yield return resource;
            }
        }

        private CollectionResource MapToResource(MovieCollection collection)
        {
            var resource = collection.ToResource();

            foreach (var movie in _movieMetadataService.GetMoviesByCollectionTmdbId(collection.TmdbId))
            {
                var movieResource = movie.ToResource();
                movieResource.Folder = _fileNameBuilder.GetMovieFolder(new Movie { MovieMetadata = movie });

                resource.Movies.Add(movieResource);
            }

            return resource;
        }

        [NonAction]
        public void Handle(CollectionAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Created, MapToResource(message.Collection));
        }

        [NonAction]
        public void Handle(CollectionEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Collection));
        }

        [NonAction]
        public void Handle(CollectionDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, MapToResource(message.Collection));
        }
    }
}
