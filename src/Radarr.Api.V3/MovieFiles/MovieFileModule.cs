using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using NLog;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Extensions;
using BadRequestException = Radarr.Http.REST.BadRequestException;

namespace Radarr.Api.V3.MovieFiles
{
    public class MovieFileModule : RadarrRestModuleWithSignalR<MovieFileResource, MovieFile>,
                                 IHandle<MovieFileAddedEvent>,
                                 IHandle<MovieFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMovieService _movieService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public MovieFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                               IMediaFileService mediaFileService,
                               IRecycleBinProvider recycleBinProvider,
                               IMovieService movieService,
                               IUpgradableSpecification qualityUpgradableSpecification,
                               Logger logger)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _recycleBinProvider = recycleBinProvider;
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;

            GetResourceById = GetMovieFile;
            GetResourceAll = GetMovieFiles;
            UpdateResource = SetMovieFile;
            DeleteResource = DeleteMovieFile;

            Put("/editor",  movieFiles => SetMovieFile());
            Delete("/bulk",  movieFiles => DeleteMovieFiles());
        }

        private MovieFileResource GetMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);

            return movieFile.ToResource(movie, _qualityUpgradableSpecification);
        }

        private List<MovieFileResource> GetMovieFiles()
        {
            var movieIdQuery = Request.Query.MovieId;
            var movieFileIdsQuery = Request.Query.MovieFileIds;

            if (!movieIdQuery.HasValue && !movieFileIdsQuery.HasValue)
            {
                throw new BadRequestException("movieId or movieFileIds must be provided");
            }

            if (movieIdQuery.HasValue)
            {
                int movieId = Convert.ToInt32(movieIdQuery.Value);
                var movie = _movieService.GetMovie(movieId);

                return _mediaFileService.GetFilesByMovie(movieId).ConvertAll(f => f.ToResource(movie, _qualityUpgradableSpecification));
            }

            else
            {
                string movieFileIdsValue = movieFileIdsQuery.Value.ToString();

                var movieFileIds = movieFileIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => Convert.ToInt32(e))
                                                        .ToList();

                var movieFiles = _mediaFileService.GetMovies(movieFileIds);

                return movieFiles.GroupBy(e => e.MovieId)
                                   .SelectMany(f => f.ToList()
                                                     .ConvertAll( e => e.ToResource(_movieService.GetMovie(f.Key), _qualityUpgradableSpecification)))
                                   .ToList();
            }
        }

        private void SetMovieFile(MovieFileResource movieFileResource)
        {
            var movieFile = _mediaFileService.GetMovie(movieFileResource.Id);
            movieFile.Quality = movieFileResource.Quality;
            movieFile.Languages = movieFileResource.Languages;
            _mediaFileService.Update(movieFile);
        }

        private object SetMovieFile()
        {
            var resource = Request.Body.FromJson<MovieFileListResource>();
            var movieFiles = _mediaFileService.GetMovies(resource.MovieFileIds);

            foreach (var movieFile in movieFiles)
            {

                if (resource.Quality != null)
                {
                    movieFile.Quality = resource.Quality;
                }
                if (resource.Languages != null)
                {
                    movieFile.Languages = resource.Languages;
                }

            }

            _mediaFileService.Update(movieFiles);

            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            return ResponseWithCode(movieFiles.ConvertAll(f => f.ToResource(movie, _qualityUpgradableSpecification))
                               , HttpStatusCode.Accepted);
        }

        private void DeleteMovieFile(int id)
        {
            var movieFile = _mediaFileService.GetMovie(id);
            var movie = _movieService.GetMovie(movieFile.MovieId);
            var fullPath = Path.Combine(movie.Path, movieFile.RelativePath);

            _logger.Info("Deleting movie file: {0}", fullPath);
            _recycleBinProvider.DeleteFile(fullPath);
            _mediaFileService.Delete(movieFile, DeleteMediaFileReason.Manual);
            // TODO: Pull MediaFileDeletionService from Sonarr
            //_mediaFileDeletionService.Delete(series, episodeFile);
        }

        private object DeleteMovieFiles()
        {
            var resource = Request.Body.FromJson<MovieFileListResource>();
            var movieFiles = _mediaFileService.GetMovies(resource.MovieFileIds);
            var movie = _movieService.GetMovie(movieFiles.First().MovieId);

            foreach (var movieFile in movieFiles)
            {
                var fullPath = Path.Combine(movie.Path, movieFile.RelativePath);
                _logger.Info("Deleting movie file: {0}", fullPath);
                _recycleBinProvider.DeleteFile(fullPath);
                _mediaFileService.Delete(movieFile, DeleteMediaFileReason.Manual);
                // TODO: Pull MediaFileDeletionService from Sonarr
                //_mediaFileDeletionService.DeleteEpisodeFile(movie, movieFile);
            }

            return new object();
        }

        public void Handle(MovieFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Id);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.MovieFile.Id);
        }
    }
}
