using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Movies
{
    public class MoveMovieService : IExecute<MoveMovieCommand>, IExecute<BulkMoveMovieCommand>
    {
        private readonly IMovieService _movieService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveMovieService(IMovieService movieService,
                                 IBuildFileNames filenameBuilder,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _movieService = movieService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void MoveSingleMovie(Movie movie, string sourcePath, string destinationPath, int? index = null, int? total = null)
        {
            if (!_diskProvider.FolderExists(sourcePath))
            {
                _logger.Debug("Folder '{0}' for '{1}' does not exist, not moving.", sourcePath, movie.Title);
                return;
            }

            if (index != null && total != null)
            {
                _logger.ProgressInfo("Moving {0} from '{1}' to '{2}' ({3}/{4})", movie.Title, sourcePath, destinationPath, index + 1, total);
            }
            else
            {
                _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", movie.Title, sourcePath, destinationPath);
            }

            if (sourcePath.PathEquals(destinationPath))
            {
                _logger.ProgressInfo("{0} is already in the specified location '{1}'.", movie, destinationPath);
                return;
            }

            try
            {
                _diskProvider.CreateFolder(new DirectoryInfo(destinationPath).Parent.FullName);
                _diskTransferService.TransferFolder(sourcePath, destinationPath, TransferMode.Move);

                _logger.ProgressInfo("{0} moved successfully to {1}", movie.Title, destinationPath);

                _eventAggregator.PublishEvent(new MovieMovedEvent(movie, sourcePath, destinationPath));
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move movie from '{0}' to '{1}'. Try moving files manually", sourcePath, destinationPath);

                RevertPath(movie.Id, sourcePath);
            }
        }

        private void RevertPath(int movieId, string path)
        {
            var movie = _movieService.GetMovie(movieId);

            movie.Path = path;
            _movieService.UpdateMovie(movie);
        }

        public void Execute(MoveMovieCommand message)
        {
            var movie = _movieService.GetMovie(message.MovieId);
            MoveSingleMovie(movie, message.SourcePath, message.DestinationPath);
        }

        public void Execute(BulkMoveMovieCommand message)
        {
            var moviesToMove = message.Movies;
            var destinationRootFolder = message.DestinationRootFolder;

            _logger.ProgressInfo("Moving {0} movies to '{1}'", moviesToMove.Count, destinationRootFolder);

            for (var index = 0; index < moviesToMove.Count; index++)
            {
                var s = moviesToMove[index];
                var movie = _movieService.GetMovie(s.MovieId);
                var destinationPath = Path.Combine(destinationRootFolder, _filenameBuilder.GetMovieFolder(movie));

                MoveSingleMovie(movie, s.SourcePath, destinationPath, index, moviesToMove.Count);
            }

            _logger.ProgressInfo("Finished moving {0} movies to '{1}'", moviesToMove.Count, destinationRootFolder);
        }
    }
}
