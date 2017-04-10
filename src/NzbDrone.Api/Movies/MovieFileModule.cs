using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Api.REST;
using NzbDrone.Api.Movie;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.SignalR;

namespace NzbDrone.Api.EpisodeFiles
{
    public class MovieFileModule : NzbDroneRestModuleWithSignalR<MovieFileResource, MovieFile>, IHandle<MovieFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMovieService _movieService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public MovieFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IRecycleBinProvider recycleBinProvider,
                             IMovieService movieService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             Logger logger)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _recycleBinProvider = recycleBinProvider;
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
            GetResourceById = GetMovieFile;
            UpdateResource = SetQuality;
            DeleteResource = DeleteMovieFile;
        }

        private MovieFileResource GetMovieFile(int id)
        {
            var movie = _mediaFileService.GetMovie(id);

            return movie.ToResource();
        }


        private void SetQuality(MovieFileResource movieFileResource)
        {  
            var movieFile = _mediaFileService.GetMovie(movieFileResource.Id);
            movieFile.Quality = movieFileResource.Quality;
            _mediaFileService.Update(movieFile);

            BroadcastResourceChange(ModelAction.Updated, movieFile.Id);
        }

        private void DeleteMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);
            var fullPath = Path.Combine(movie.Path, movieFile.RelativePath);

            _logger.Info("Deleting movie file: {0}", fullPath);
            _recycleBinProvider.DeleteFile(fullPath);
            _mediaFileService.Delete(movieFile, DeleteMediaFileReason.Manual);
        }

        public void Handle(MovieFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }
    }
}