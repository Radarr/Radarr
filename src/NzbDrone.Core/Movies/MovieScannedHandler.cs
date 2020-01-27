﻿using System.Collections.Generic;
using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies
{
    public class MovieScannedHandler : IHandle<MovieScannedEvent>,
                                        IHandle<MovieScanSkippedEvent>
    {
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;

        private readonly Logger _logger;

        public MovieScannedHandler(IMovieService movieService,
                                    IManageCommandQueue commandQueueManager,
                                    Logger logger)
        {
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        private void HandleScanEvents(Movie movie)
        {
            if (movie.AddOptions == null)
            {
                //_episodeAddedService.SearchForRecentlyAdded(movie.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", movie.Title);

            //_episodeMonitoredService.SetEpisodeMonitoredStatus(movie, movie.AddOptions);
            if (movie.AddOptions.SearchForMovie)
            {
                _commandQueueManager.Push(new MoviesSearchCommand { MovieIds = new List<int> { movie.Id } });
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
