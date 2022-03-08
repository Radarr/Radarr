using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.Collections
{
    public interface IAddMovieCollectionService
    {
        MovieCollection AddMovieCollection(MovieCollection newCollection);
    }

    public class AddMovieCollectionService : IAddMovieCollectionService
    {
        private readonly IMovieCollectionService _collectionService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly Logger _logger;

        public AddMovieCollectionService(IMovieCollectionService collectionService,
                                IProvideMovieInfo movieInfo,
                                Logger logger)
        {
            _collectionService = collectionService;
            _movieInfo = movieInfo;
            _logger = logger;
        }

        public MovieCollection AddMovieCollection(MovieCollection newCollection)
        {
            Ensure.That(newCollection, () => newCollection).IsNotNull();

            var existingCollection = _collectionService.FindByTmdbId(newCollection.TmdbId);

            if (existingCollection != null)
            {
                return existingCollection;
            }

            newCollection = AddSkyhookData(newCollection);
            newCollection = SetPropertiesAndValidate(newCollection);

            _logger.Info("Adding Collection {0}", newCollection);

            _collectionService.AddCollection(newCollection);

            return newCollection;
        }

        private MovieCollection AddSkyhookData(MovieCollection newCollection)
        {
            MovieCollection collection;

            try
            {
                collection = _movieInfo.GetCollectionInfo(newCollection.TmdbId);
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("TmdbId {0} was not found, it may have been removed from TMDb.", newCollection.TmdbId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TmdbId", $"A collection with this ID was not found.", newCollection.TmdbId)
                                              });
            }

            collection.ApplyChanges(newCollection);

            return collection;
        }

        private MovieCollection SetPropertiesAndValidate(MovieCollection newCollection)
        {
            newCollection.CleanTitle = newCollection.Title.CleanMovieTitle();
            newCollection.SortTitle = MovieTitleNormalizer.Normalize(newCollection.Title, newCollection.TmdbId);
            newCollection.Added = DateTime.UtcNow;

            return newCollection;
        }
    }
}
