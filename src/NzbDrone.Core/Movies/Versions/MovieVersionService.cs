using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Versions
{
    public interface IMovieVersionService
    {
        MovieVersion Get(int id);
        List<MovieVersion> GetByMovieId(int movieId);
        List<MovieVersion> GetByMovieIds(List<int> movieIds);
        MovieVersion FindByPath(string path);
        MovieVersion Add(MovieVersion version);
        MovieVersion Update(MovieVersion version);
        void Delete(int id);
    }

    public class MovieVersionService : IMovieVersionService,
                                       IHandle<MovieAddedEvent>,
                                       IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMovieVersionRepository _repository;
        private readonly Logger _logger;

        public MovieVersionService(IMovieVersionRepository repository, Logger logger)
        {
            _repository = repository;
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

        public MovieVersion Add(MovieVersion version)
        {
            return _repository.Insert(version);
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

        public void HandleAsync(MoviesDeletedEvent message)
        {
            _repository.DeleteForMovies(message.Movies.Select(m => m.Id).ToList());
        }
    }
}
