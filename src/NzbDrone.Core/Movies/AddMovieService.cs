using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieService
    {
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies);
    }

    public class AddMovieService : IAddMovieService
    {
        private readonly IMovieService _movieService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddMovieValidator _addMovieValidator;
        private readonly Logger _logger;

        public AddMovieService(IMovieService movieService,
                                IProvideMovieInfo movieInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddMovieValidator addMovieValidator,
                                Logger logger)
        {
            _movieService = movieService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _addMovieValidator = addMovieValidator;
            _logger = logger;
        }

        public Movie AddMovie(Movie newMovie)
        {
            Ensure.That(newMovie, () => newMovie).IsNotNull();

            newMovie = AddSkyhookData(newMovie);
            newMovie = SetPropertiesAndValidate(newMovie);

            _logger.Info("Adding Movie {0} Path: [{1}]", newMovie, newMovie.Path);
            _movieService.AddMovie(newMovie);

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies)
        {
            var added = DateTime.UtcNow;
            var moviesToAdd = new List<Movie>();

            foreach (var m in newMovies)
            {
                // TODO: Verify if adding skyhook data will be slow
                var movie = AddSkyhookData(m);
                movie = SetPropertiesAndValidate(movie);
                movie.Added = added;
                moviesToAdd.Add(movie);
            }

            return _movieService.AddMovies(moviesToAdd);
        }

        private Movie AddSkyhookData(Movie newMovie)
        {
            Movie movie;

            try
            {
                movie = _movieInfo.GetMovieInfo(newMovie.TmdbId).Item1;
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("TmdbId {1} was not found, it may have been removed from TMDb.", newMovie.TmdbId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", "A movie with this ID was not found", newMovie.TmdbId)
                                              });
            }

            movie.ApplyChanges(newMovie);

            return movie;
        }

        private Movie SetPropertiesAndValidate(Movie newMovie)
        {
            if (string.IsNullOrWhiteSpace(newMovie.Path))
            {
                var folderName = _fileNameBuilder.GetMovieFolder(newMovie);
                newMovie.Path = Path.Combine(newMovie.RootFolderPath, folderName);
            }

            newMovie.CleanTitle = newMovie.Title.CleanSeriesTitle();
            newMovie.SortTitle = MovieTitleNormalizer.Normalize(newMovie.Title, newMovie.TmdbId);
            newMovie.Added = DateTime.UtcNow;

            var validationResult = _addMovieValidator.Validate(newMovie);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newMovie;
        }
    }
}
