using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Http;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace NzbDrone.Api.MovieFiles
{
    public class MovieFileModule : RadarrRestModuleWithSignalR<MovieFileResource, MovieFile>, IHandle<MovieFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IMovieService _movieService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;

        public MovieFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDeleteMediaFiles mediaFileDeletionService,
                             IMovieService movieService,
                             IUpgradableSpecification qualityUpgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
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

            if (movieFile == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Movie file not found");
            }

            var movie = _movieService.GetMovie(movieFile.MovieId);

            _mediaFileDeletionService.DeleteMovieFile(movie, movieFile);
        }

        public void Handle(MovieFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }
    }
}
