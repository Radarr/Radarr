using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;
using System;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        MovieFile Add(MovieFile movieFile);
        void Update(MovieFile movieFile);
        void Delete(MovieFile movieFile, DeleteMediaFileReason reason);
        EpisodeFile Add(EpisodeFile episodeFile);
        void Update(EpisodeFile episodeFile);
        void Delete(EpisodeFile episodeFile, DeleteMediaFileReason reason);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<MovieFile> GetFilesByMovie(int movieId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<EpisodeFile> GetFilesWithoutMediaInfo();
        List<string> FilterExistingFiles(List<string> files, Series series);
        List<string> FilterExistingFiles(List<string> files, Movie movie);
        EpisodeFile Get(int id);
        MovieFile GetMovie(int id);
        List<EpisodeFile> Get(IEnumerable<int> ids);
        List<MovieFile> GetMovies(IEnumerable<int> ids);

        //List<MovieFile> Get(IEnumerable<int> ids);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly IMovieFileRepository _movieFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IMovieFileRepository movieFileRepository, IEventAggregator eventAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _eventAggregator = eventAggregator;
            _movieFileRepository = movieFileRepository;
            _logger = logger;
        }

        public EpisodeFile Add(EpisodeFile episodeFile)
        {
            var addedFile = _mediaFileRepository.Insert(episodeFile);
            _eventAggregator.PublishEvent(new EpisodeFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(EpisodeFile episodeFile)
        {
            _mediaFileRepository.Update(episodeFile);
        }

        public void Delete(EpisodeFile episodeFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the episodes and series attached for the event consumers
            episodeFile.Episodes.LazyLoad();
            episodeFile.Path = Path.Combine(episodeFile.Series.Value.Path, episodeFile.RelativePath);

            _mediaFileRepository.Delete(episodeFile);
            _eventAggregator.PublishEvent(new EpisodeFileDeletedEvent(episodeFile, reason));
        }

        public void Delete(MovieFile episodeFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the episodes and series attached for the event consumers
            episodeFile.Movie.LazyLoad();
            episodeFile.Path = Path.Combine(episodeFile.Movie.Value.Path, episodeFile.RelativePath);

            _movieFileRepository.Delete(episodeFile);
            _eventAggregator.PublishEvent(new MovieFileDeletedEvent(episodeFile, reason));
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return _mediaFileRepository.GetFilesBySeries(seriesId);
        }

        public List<MovieFile> GetFilesByMovie(int movieId)
        {
            return _movieFileRepository.GetFilesByMovie(movieId); //TODO: Update implementation for movie files.
        }

        public List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return _mediaFileRepository.GetFilesBySeason(seriesId, seasonNumber);
        }

        public List<EpisodeFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Series series)
        {
            var seriesFiles = GetFilesBySeries(series.Id).Select(f => Path.Combine(series.Path, f.RelativePath)).ToList();

            if (!seriesFiles.Any()) return files;

            return files.Except(seriesFiles, PathEqualityComparer.Instance).ToList();
        }

        public List<string> FilterExistingFiles(List<string> files, Movie movie)
        {
            var movieFiles = GetFilesByMovie(movie.Id).Select(f => Path.Combine(movie.Path, f.RelativePath)).ToList();

            if (!movieFiles.Any()) return files;

            return files.Except(movieFiles, PathEqualityComparer.Instance).ToList();
        }

        public EpisodeFile Get(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<EpisodeFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public List<MovieFile> GetMovies(IEnumerable<int> ids)
        {
            return _movieFileRepository.Get(ids).ToList();
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var files = GetFilesBySeries(message.Series.Id);
            _mediaFileRepository.DeleteMany(files);
        }

        public MovieFile Add(MovieFile episodeFile)
        {
            var addedFile = _movieFileRepository.Insert(episodeFile);
            _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(MovieFile episodeFile)
        {
            _movieFileRepository.Update(episodeFile);
        }

        public MovieFile GetMovie(int id)
        {
            return _movieFileRepository.Get(id);
        }
    }
}