using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Translations
{
    public interface IMovieTranslationService
    {
        List<MovieTranslation> GetAllTranslationsForMovie(int movieId);
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

        public List<MovieTranslation> GetAllTranslationsForMovie(int movieId)
        {
            return _translationRepo.FindByMovieMetadataId(movieId).ToList();
        }

        public List<MovieTranslation> GetAllTranslationsForLanguage(Language language)
        {
            return _translationRepo.FindByLanguage(language).ToList();
        }

        public void RemoveTitle(MovieTranslation title)
        {
            _translationRepo.Delete(title);
        }

        public List<MovieTranslation> UpdateTranslations(List<MovieTranslation> translations, MovieMetadata movie)
        {
            int movieId = movie.Id;

            // First update the movie ids so we can correlate them later
            translations.ForEach(t => t.MovieMetadataId = movieId);

            // Then throw out any we don't have languages for
            translations = translations.Where(t => t.Language != null).ToList();

            // Then make sure they are all distinct languages
            translations = translations.DistinctBy(t => t.Language).ToList();

            // Now find translations to delete, update and insert
            var existingTranslations = _translationRepo.FindByMovieMetadataId(movieId);

            translations.ForEach(c => c.Id = existingTranslations.FirstOrDefault(t => t.Language == c.Language)?.Id ?? 0);

            var insert = translations.Where(t => t.Id == 0).ToList();
            var update = translations.Where(t => t.Id > 0).ToList();
            var delete = existingTranslations.Where(t => !translations.Any(c => c.Language == t.Language)).ToList();

            _translationRepo.DeleteMany(delete.ToList());
            _translationRepo.UpdateMany(update.ToList());
            _translationRepo.InsertMany(insert.ToList());

            return translations;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO hanlde metadata delete instead of movie delete
            _translationRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
