using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.Translations
{
    public interface IMovieTranslationRepository : IBasicRepository<MovieTranslation>
    {
        List<MovieTranslation> FindByMovieMetadataId(int movieId);
        List<MovieTranslation> FindByLanguage(Language language);
        void DeleteForMovies(List<int> movieIds);
    }

    public class MovieTranslationRepository : BasicRepository<MovieTranslation>, IMovieTranslationRepository
    {
        public MovieTranslationRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MovieTranslation> FindByMovieMetadataId(int movieId)
        {
            return Query(x => x.MovieMetadataId == movieId);
        }

        public List<MovieTranslation> FindByLanguage(Language language)
        {
            return Query(x => x.Language == language);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieMetadataId));
        }
    }
}
