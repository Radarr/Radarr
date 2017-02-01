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
    public class MovieFileModule : NzbDroneRestModuleWithSignalR<MovieFileResource, MovieFile>
                                 //IHandle<EpisodeFileAddedEvent>
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
            /*GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;*/
            UpdateResource = SetQuality;
            DeleteResource = DeleteEpisodeFile;
        }

        private MovieFileResource GetMovieFile(int id)
        {
            var episodeFile = _mediaFileService.GetMovie(id);

            return episodeFile.ToResource();
        }

        /*private List<EpisodeFileResource> GetEpisodeFiles()
        {
            if (!Request.Query.SeriesId.HasValue)
            {
                throw new BadRequestException("seriesId is missing");
            }

            var seriesId = (int)Request.Query.SeriesId;

            var series = _seriesService.GetSeries(seriesId);

            return _mediaFileService.GetFilesBySeries(seriesId).ConvertAll(f => f.ToResource(series, _qualityUpgradableSpecification));
        }
        */
        private void SetQuality(MovieFileResource movieFileResource)
        {  
            var movieFile = _mediaFileService.GetMovie(movieFileResource.Id);
            movieFile.Quality = movieFileResource.Quality;
            _mediaFileService.Update(movieFile);

            BroadcastResourceChange(ModelAction.Updated, movieFile.Id);
        }

        //TODO
        private void DeleteEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.GetMovie(id);
            var series = _movieService.GetMovie(episodeFile.MovieId);
            var fullPath = Path.Combine(series.Path, episodeFile.RelativePath);

            _logger.Info("Deleting episode file: {0}", fullPath);
            _recycleBinProvider.DeleteFile(fullPath);
            _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.Manual);
        }

        public void Handle(MovieFileUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.Id);
        }
    }
}