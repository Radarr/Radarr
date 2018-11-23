using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Common;
using System;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        MovieFile Add(MovieFile movieFile);
        void Update(MovieFile movieFile);
        void Update(List<MovieFile> movieFile);
        void Delete(MovieFile movieFile, DeleteMediaFileReason reason);
        List<MovieFile> GetFilesByMovie(int movieId);
        List<MovieFile> GetFilesWithoutMediaInfo();
        List<string> FilterExistingFiles(List<string> files, Movie movie);
        MovieFile GetMovie(int id);
        List<MovieFile> GetMovies(IEnumerable<int> ids);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<MovieDeletedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IMovieService movieService,
                                IEventAggregator eventAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _eventAggregator = eventAggregator;
            _movieService = movieService;
            _logger = logger;
        }

        public MovieFile Add(MovieFile movieFile)
        {
            var addedFile = _mediaFileRepository.Insert(movieFile);
            addedFile.Movie.LazyLoad();
            if (addedFile.Movie == null || addedFile.Movie.Value == null)
            {
                _logger.Error("Movie is null for the file {0}. Please run the houskeeping command to ensure movies and files are linked correctly.");
            }
            //_movieService.SetFileId(addedFile.Movie.Value, addedFile); //Should not be necessary, but sometimes below fails?
            _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));

            return addedFile;
        }

        public void Update(MovieFile movieFile)
        {
            _mediaFileRepository.Update(movieFile);
        }

        public void Update(List<MovieFile> movieFiles)
        {
            _mediaFileRepository.UpdateMany(movieFiles);
        }

        public void Delete(MovieFile movieFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the movie attached for the event consumers
            movieFile.Movie.LazyLoad();
            movieFile.Path = Path.Combine(movieFile.Movie.Value.Path, movieFile.RelativePath);

            _mediaFileRepository.Delete(movieFile);
            _eventAggregator.PublishEvent(new MovieFileDeletedEvent(movieFile, reason));
        }

        public List<MovieFile> GetFilesByMovie(int movieId)
        {
            return _mediaFileRepository.GetFilesByMovie(movieId);
        }

        public List<MovieFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Movie movie)
        {
            var movieFiles = GetFilesByMovie(movie.Id).Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList();

            if (!movieFiles.Any()) return files;

            return files.Except(movieFiles, PathEqualityComparer.Instance).ToList();
        }

        public List<MovieFile> GetMovies(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public MovieFile GetMovie(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public void HandleAsync(MovieDeletedEvent message)
        {

            var files = GetFilesByMovie(message.Movie.Id);
            _mediaFileRepository.DeleteMany(files);

        }
    }
}
