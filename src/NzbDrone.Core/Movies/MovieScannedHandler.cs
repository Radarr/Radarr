using System.Collections.Generic;
using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Collections;

namespace NzbDrone.Core.Movies
{
    public class MovieScannedHandler : IHandle<MovieScannedEvent>,
                                        IHandle<MovieScanSkippedEvent>
    {
        private readonly IMovieService _movieService;
        private readonly IMovieCollectionService _collectionService;
        private readonly IManageCommandQueue _commandQueueManager;

        private readonly Logger _logger;

        public MovieScannedHandler(IMovieService movieService,
                                    IMovieCollectionService collectionService,
                                    IManageCommandQueue commandQueueManager,
                                    Logger logger)
        {
            _movieService = movieService;
            _collectionService = collectionService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        private void HandleScanEvents(Movie movie)
        {
            if (movie.AddOptions == null)
            {
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", movie.Title);

            if (movie.AddOptions.SearchForMovie)
            {
                _commandQueueManager.Push(new MoviesSearchCommand { MovieIds = new List<int> { movie.Id } });
            }

            if (movie.AddOptions.Monitor == MonitorTypes.MovieAndCollection && movie.MovieMetadata.Value.CollectionTmdbId > 0)
            {
                var collection = _collectionService.FindByTmdbId(movie.MovieMetadata.Value.CollectionTmdbId);
                collection.Monitored = true;

                _collectionService.UpdateCollection(collection);
            }

            movie.AddOptions = null;
            _movieService.RemoveAddOptions(movie);
        }

        public void Handle(MovieScannedEvent message)
        {
            HandleScanEvents(message.Movie);
        }

        public void Handle(MovieScanSkippedEvent message)
        {
            HandleScanEvents(message.Movie);
        }
    }
}
