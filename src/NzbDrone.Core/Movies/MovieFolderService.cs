using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public interface IMovieFolderService
    {
        string GetExpectedMovieFolder(Movie movie, NamingConfig namingConfig);
        bool TryMoveMovieFolder(Movie movie, string sourcePath, string destinationPath);
    }

    public class MovieFolderService : IMovieFolderService
    {
        private readonly IMovieService _movieService;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly IRootFolderService _rootFolderService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MovieFolderService(IMovieService movieService,
                                  IBuildMoviePaths moviePathBuilder,
                                  IRootFolderService rootFolderService,
                                  IDiskProvider diskProvider,
                                  IDiskTransferService diskTransferService,
                                  IEventAggregator eventAggregator,
                                  Logger logger)
        {
            _movieService = movieService;
            _moviePathBuilder = moviePathBuilder;
            _rootFolderService = rootFolderService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public string GetExpectedMovieFolder(Movie movie, NamingConfig namingConfig)
        {
            if (!namingConfig.RenameMovies)
            {
                return null;
            }

            movie.RootFolderPath = _rootFolderService.GetBestRootFolderPath(movie.Path);

            return _moviePathBuilder.BuildPath(movie, false);
        }

        public bool TryMoveMovieFolder(Movie movie, string sourcePath, string destinationPath)
        {
            if (sourcePath.PathEquals(destinationPath))
            {
                return true;
            }

            try
            {
                _diskProvider.CreateFolder(new DirectoryInfo(destinationPath).Parent.FullName);
                _diskTransferService.TransferFolder(sourcePath, destinationPath, TransferMode.Move);

                movie.Path = destinationPath;
                _movieService.UpdateMovie(movie);

                _logger.ProgressInfo("Renamed movie folder from '{0}' to '{1}'", sourcePath, destinationPath);

                _eventAggregator.PublishEvent(new MovieMovedEvent(movie, sourcePath, destinationPath));

                return true;
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move movie folder from '{0}' to '{1}'. Try moving files manually", sourcePath, destinationPath);

                return false;
            }
        }
    }
}
