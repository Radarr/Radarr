using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.ChangeTracker;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Translations
{
    public interface IMovieTranslationService
    {
        List<MovieTranslation> GetAllTranslationsForMovieMetadata(int movieMetadataId);
        List<MovieTranslation> GetAllTranslationsForLanguage(Language language);
        List<MovieTranslation> UpdateTranslations(List<MovieTranslation> titles, MovieMetadata movie);
    }

    public class MovieTranslationService : IMovieTranslationService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IMovieTranslationRepository _translationRepo;
        private readonly Logger _logger;

        public MovieTranslationService(IMovieTranslationRepository translationRepo,
                             Logger logger)
        {
            _translationRepo = translationRepo;
            _logger = logger;
        }

        public List<MovieTranslation> GetAllTranslationsForMovieMetadata(int movieMetadataId)
        {
            return _translationRepo.FindByMovieMetadataId(movieMetadataId).ToList();
        }

        public List<MovieTranslation> GetAllTranslationsForLanguage(Language language)
        {
            return _translationRepo.FindByLanguage(language).ToList();
        }

        public void RemoveTitle(MovieTranslation title)
        {
            _translationRepo.Delete(title);
        }

        public List<MovieTranslation> UpdateTranslations(List<MovieTranslation> translations, MovieMetadata movieMetadata)
        {
            var movieMetadataId = movieMetadata.Id;

            // First update the movie ids so we can correlate them later
            translations.ForEach(t => t.MovieMetadataId = movieMetadataId);

            // Then throw out any we don't have languages for
            translations = translations.Where(t => t.Language != null).ToList();

            // Then make sure they are all distinct languages
            translations = translations.DistinctBy(t => t.Language).ToList();

            // Now find translations to delete, update and insert
            var existingTranslations = _translationRepo.FindByMovieMetadataId(movieMetadataId);

            ChangeTracker<MovieTranslation>.DetectChanges(translations, existingTranslations, t => t.Language, out var insert, out var update, out var delete);

            _logger.Debug("UpdateTranslation({0}): [{1}] inserts, [{2}] updates, [{3}] deletes", translations.Count, insert.Count, update.Count, delete.Count);
            _translationRepo.DeleteMany(delete);
            _translationRepo.UpdateMany(update);
            _translationRepo.InsertMany(insert);

            return translations;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO handle metadata delete instead of movie delete
            _translationRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
