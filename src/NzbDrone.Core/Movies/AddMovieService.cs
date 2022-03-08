using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieService
    {
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies, bool ignoreErrors = false);
    }

    public class AddMovieService : IAddMovieService
    {
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddMovieValidator _addMovieValidator;
        private readonly Logger _logger;

        public AddMovieService(IMovieService movieService,
                                IMovieMetadataService movieMetadataService,
                                IProvideMovieInfo movieInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddMovieValidator addMovieValidator,
                                Logger logger)
        {
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
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

            _movieMetadataService.Upsert(newMovie.MovieMetadata.Value);
            newMovie.MovieMetadataId = newMovie.MovieMetadata.Value.Id;

            _movieService.AddMovie(newMovie);

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var moviesToAdd = new List<Movie>();

            foreach (var m in newMovies)
            {
                _logger.Info("Adding Movie {0} Path: [{1}]", m, m.Path);

                try
                {
                    var movie = AddSkyhookData(m);
                    movie = SetPropertiesAndValidate(movie);

                    movie.Added = added;

                    moviesToAdd.Add(movie);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("TmdbId {0} was not added due to validation failures. {1}", m.TmdbId, ex.Message);
                }
            }

            _movieMetadataService.UpsertMany(moviesToAdd.Select(x => x.MovieMetadata.Value).ToList());
            moviesToAdd.ForEach(x => x.MovieMetadataId = x.MovieMetadata.Value.Id);

            return _movieService.AddMovies(moviesToAdd);
        }

        private Movie AddSkyhookData(Movie newMovie)
        {
            var movie = new Movie();

            try
            {
                movie.MovieMetadata = _movieInfo.GetMovieInfo(newMovie.TmdbId).Item1;
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("TmdbId {0} was not found, it may have been removed from TMDb. Path: {1}", newMovie.TmdbId, newMovie.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", $"A movie with this ID was not found. Path: {newMovie.Path}", newMovie.TmdbId)
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

            newMovie.MovieMetadata.Value.CleanTitle = newMovie.Title.CleanMovieTitle();
            newMovie.MovieMetadata.Value.SortTitle = MovieTitleNormalizer.Normalize(newMovie.Title, newMovie.TmdbId);
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
