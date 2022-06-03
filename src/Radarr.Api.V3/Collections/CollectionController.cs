using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
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

        public CollectionController(IBroadcastSignalRMessage signalRBroadcaster,
                                    IMovieCollectionService collectionService,
                                    IMovieService movieService,
                                    IMovieMetadataService movieMetadataService,
                                    IBuildFileNames fileNameBuilder,
                                    INamingConfigService namingService)
            : base(signalRBroadcaster)
        {
            _collectionService = collectionService;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
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
        public ActionResult UpdateCollections(CollectionUpdateResource collectionResources)
        {
            var collectionsToUpdate = _collectionService.GetCollections(collectionResources.Collections.Select(c => c.Id));
            var update = new List<CollectionResource>();

            foreach (var c in collectionResources.Collections)
            {
                var collection = collectionsToUpdate.Single(n => n.Id == c.Id);

                if (c.Monitored.HasValue)
                {
                    collection.Monitored = c.Monitored.Value;
                }

                if (collectionResources.MonitorMovies.HasValue)
                {
                    var movies = _movieService.GetMoviesByCollectionTmdbId(collection.TmdbId);

                    movies.ForEach(c => c.Monitored = collectionResources.MonitorMovies.Value);

                    _movieService.UpdateMovie(movies, true);
                }

                var updatedCollection = _collectionService.UpdateCollection(collection);
                update.Add(updatedCollection.ToResource());
            }

            return Accepted(update);
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
                movieResource.Folder = _fileNameBuilder.GetMovieFolder(new Movie { Title = movie.Title, Year = movie.Year, ImdbId = movie.ImdbId, TmdbId = movie.TmdbId });

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
