using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Movies.Versions
{
    public interface IMovieVersionService
    {
        MovieVersion Get(int id);
        List<MovieVersion> GetByMovieId(int movieId);
        List<MovieVersion> GetByMovieIds(List<int> movieIds);
        MovieVersion FindByPath(string path);
        MovieVersion GetPrimary(int movieId);
        IEnumerable<Movie> GetAdditionalVersionViews(Movie movie);
        IEnumerable<int> GetAdditionalVersionIds(int movieId);
        Movie GetMovieForFile(Movie movie, MovieFile movieFile);
        Movie GetVersionView(Movie movie, int movieVersionId);
        MovieVersion Add(MovieVersion version);
        MovieVersion AddVersion(Movie movie, int qualityProfileId, string rootFolderPath);
        void RemoveVersion(int id);
        MovieVersion Update(MovieVersion version);
        void Delete(int id);
    }

    public class MovieVersionService : IMovieVersionService,
                                       IHandle<MovieAddedEvent>,
                                       IHandle<MovieEditedEvent>,
                                       IHandle<MoviesBulkEditedEvent>,
                                       IHandle<MovieMovedEvent>,
                                       IHandle<MovieFileAddedEvent>,
                                       IHandle<MovieFileDeletedEvent>,
                                       IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMovieVersionRepository _repository;
        private readonly IMovieRepository _movieRepository;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly IQualityProfileRepository _qualityProfileRepository;
        private readonly Logger _logger;

        public MovieVersionService(IMovieVersionRepository repository,
                                   IMovieRepository movieRepository,
                                   IMediaFileRepository mediaFileRepository,
                                   IQualityProfileRepository qualityProfileRepository,
                                   Logger logger)
        {
            _repository = repository;
            _movieRepository = movieRepository;
            _mediaFileRepository = mediaFileRepository;
            _qualityProfileRepository = qualityProfileRepository;
            _logger = logger;
        }

        public MovieVersion Get(int id)
        {
            return _repository.Get(id);
        }

        public List<MovieVersion> GetByMovieId(int movieId)
        {
            return _repository.FindByMovieId(movieId);
        }

        public List<MovieVersion> GetByMovieIds(List<int> movieIds)
        {
            return _repository.FindByMovieIds(movieIds);
        }

        public MovieVersion FindByPath(string path)
        {
            return _repository.FindByPath(path);
        }

        // The primary version mirrors Movies.Path/QualityProfileId/MovieFileId and is the oldest one
        public MovieVersion GetPrimary(int movieId)
        {
            return _repository.FindByMovieId(movieId).OrderBy(v => v.Id).FirstOrDefault();
        }

        public IEnumerable<Movie> GetAdditionalVersionViews(Movie movie)
        {
            return GetAdditionalVersions(movie.Id).Select(v => BuildView(movie, v)).ToList();
        }

        public IEnumerable<int> GetAdditionalVersionIds(int movieId)
        {
            return GetAdditionalVersions(movieId).Select(v => v.Id).ToList();
        }

        public Movie GetMovieForFile(Movie movie, MovieFile movieFile)
        {
            var versionId = movieFile.GetVersionId(GetAdditionalVersionIds(movie.Id));

            if (versionId == movie.MovieVersionId)
            {
                return movie;
            }

            if (versionId == 0)
            {
                return _movieRepository.Get(movie.Id);
            }

            var version = _repository.Get(versionId);

            return BuildView(movie, version);
        }

        public Movie GetVersionView(Movie movie, int movieVersionId)
        {
            return BuildView(movie, _repository.Get(movieVersionId));
        }

        public MovieVersion Add(MovieVersion version)
        {
            return _repository.Insert(version);
        }

        // Without a root folder the version shares the movie's folder, otherwise it uses the movie's folder name in that root folder
        public MovieVersion AddVersion(Movie movie, int qualityProfileId, string rootFolderPath)
        {
            if (_qualityProfileRepository.Get(qualityProfileId) == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Quality profile {0} does not exist", qualityProfileId);
            }

            var path = rootFolderPath.IsNullOrWhiteSpace()
                ? movie.Path
                : Path.Combine(rootFolderPath, new DirectoryInfo(movie.Path).Name);

            var existing = _repository.FindByMovieId(movie.Id);

            if (existing.Any(v => v.QualityProfileId == qualityProfileId && v.Path.PathEquals(path)))
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "{0} already has a version with this quality profile in {1}", movie.Title, path);
            }

            _logger.Info("Adding version of {0} with quality profile {1} in {2}", movie, qualityProfileId, path);

            return _repository.Insert(new MovieVersion
            {
                MovieId = movie.Id,
                QualityProfileId = qualityProfileId,
                Path = path
            });
        }

        // The primary version can't be removed, and a version keeps its file on disk so it must be deleted first
        public void RemoveVersion(int id)
        {
            var version = _repository.Get(id);
            var primary = GetPrimary(version.MovieId);

            if (primary.Id == version.Id)
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "The primary version of a movie can't be removed");
            }

            if (version.HasFile)
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Delete the file of this version before removing it");
            }

            _repository.Delete(version);
        }

        public MovieVersion Update(MovieVersion version)
        {
            return _repository.Update(version);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public void Handle(MovieAddedEvent message)
        {
            var movie = message.Movie;

            if (_repository.FindByMovieId(movie.Id).Any())
            {
                return;
            }

            _logger.Debug("Creating primary version for {0}", movie);

            _repository.Insert(new MovieVersion
            {
                MovieId = movie.Id,
                QualityProfileId = movie.QualityProfileId,
                Path = movie.Path,
                MovieFileId = movie.MovieFileId
            });
        }

        public void Handle(MovieEditedEvent message)
        {
            SyncPrimary(message.Movie);
        }

        public void Handle(MoviesBulkEditedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                SyncPrimary(movie);
            }
        }

        public void Handle(MovieMovedEvent message)
        {
            SyncPrimary(message.Movie);
        }

        public void Handle(MovieFileAddedEvent message)
        {
            var movieFile = message.MovieFile;

            var version = movieFile.MovieVersionId > 0
                ? _repository.Get(movieFile.MovieVersionId)
                : GetPrimary(movieFile.MovieId);

            if (version == null)
            {
                return;
            }

            version.MovieFileId = movieFile.Id;
            _repository.Update(version);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            var versions = _repository.FindByMovieId(message.MovieFile.MovieId)
                                      .Where(v => v.MovieFileId == message.MovieFile.Id)
                                      .ToList();

            foreach (var version in versions)
            {
                version.MovieFileId = 0;
            }

            _repository.UpdateMany(versions);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            _repository.DeleteForMovies(message.Movies.Select(m => m.Id).ToList());
        }

        private List<MovieVersion> GetAdditionalVersions(int movieId)
        {
            return _repository.FindByMovieId(movieId).OrderBy(v => v.Id).Skip(1).ToList();
        }

        private Movie BuildView(Movie movie, MovieVersion version)
        {
            var view = movie.ForVersion(version);

            view.QualityProfile = _qualityProfileRepository.Get(version.QualityProfileId);

            if (version.MovieFileId > 0)
            {
                view.MovieFile = _mediaFileRepository.Get(version.MovieFileId);
            }

            return view;
        }

        private void SyncPrimary(Movie movie)
        {
            if (movie == null || movie.IsVersionView)
            {
                return;
            }

            var primary = GetPrimary(movie.Id);

            if (primary == null)
            {
                return;
            }

            if (primary.Path == movie.Path && primary.QualityProfileId == movie.QualityProfileId)
            {
                return;
            }

            primary.Path = movie.Path;
            primary.QualityProfileId = movie.QualityProfileId;
            _repository.Update(primary);
        }
    }
}
