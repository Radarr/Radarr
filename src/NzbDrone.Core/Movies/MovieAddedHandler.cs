using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Movies
{
    public class MovieAddedHandler : IHandle<MovieAddedEvent>, IHandle<MoviesImportedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IMovieFolderService _movieFolderService;
        private readonly INamingConfigService _namingConfigService;

        public MovieAddedHandler(IManageCommandQueue commandQueueManager,
                                  IMovieFolderService movieFolderService,
                                  INamingConfigService namingConfigService)
        {
            _commandQueueManager = commandQueueManager;
            _movieFolderService = movieFolderService;
            _namingConfigService = namingConfigService;
        }

        // Rename the movie folder (if requested) before the initial scan runs, so the scan
        // discovers files directly in their final location instead of the folder being moved
        // out from under an already-completed scan.
        private void RenameFolderIfRequested(Movie movie)
        {
            if (movie.AddOptions is not { RenameFolderOnImport: true })
            {
                return;
            }

            var namingConfig = _namingConfigService.GetConfig();
            var expectedFolder = _movieFolderService.GetExpectedMovieFolder(movie, namingConfig);

            if (expectedFolder != null && !movie.Path.PathEquals(expectedFolder, StringComparison.Ordinal))
            {
                _movieFolderService.TryMoveMovieFolder(movie, movie.Path, expectedFolder);
            }
        }

        public void Handle(MovieAddedEvent message)
        {
            RenameFolderIfRequested(message.Movie);

            _commandQueueManager.Push(new RefreshMovieCommand(new List<int> { message.Movie.Id }, true));
        }

        public void Handle(MoviesImportedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                RenameFolderIfRequested(movie);
            }

            _commandQueueManager.PushMany(message.Movies.Select(s => new RefreshMovieCommand(new List<int> { s.Id }, true)).ToList());
        }
    }
}
